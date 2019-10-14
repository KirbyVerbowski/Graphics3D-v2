using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using System.Drawing;
using System.Windows.Forms;

namespace Graphics3D_v2
{
    public class Object3D
    {
        public List<Type> components = new List<Type>();
        public Dictionary<Type, FieldInfo[]> componentFieldInfo = new Dictionary<Type, FieldInfo[]>();
        public Dictionary<Type, object[]> initialVals = new Dictionary<Type, object[]>();

        public SceneManager scene;
        public bool enabled = true; //Will this object be affected by scenemanager (start / update)
        public bool visible = true;
        public Transform transform;
        public string name = "unnamed object";

        public Object3D(SceneManager scene)
        {
            this.scene = scene;
            this.transform = new Transform();
        }
        public Object3D(SceneManager scene, Transform transform)
        {
            this.scene = scene;
            this.transform = transform;
        }
        public Object3D(SceneManager scene, Transform transform, string name)
        {
            this.scene = scene;
            this.transform = transform;
            this.name = name;
        }

        public void AddComponent(Component component)
        {
            foreach (Type c in components)
            {
                if (component.GetType() == c)
                {
                    throw new Exception("Can only have one component per type");
                }
            }
            Type compType = component.GetType();
            components.Add(compType);
            componentFieldInfo.Add(compType, compType.GetFields());
            object[] vals = new object[componentFieldInfo[compType].Length];
            for(int i = 0; i < vals.Length; i++)
            {
                vals[i] = componentFieldInfo[compType][i].GetValue(component);
            }
            initialVals.Add(compType, vals);
        }
        public void RemoveComponent(Type component)
        {
            foreach (Type c in components)
            {
                if(component == c){
                    components.Remove(c);
                    initialVals.Remove(c);
                    return;
                }
            }
        }

        public object GetComponent(Type component)
        {
            return scene.GetComponent(this, component);
        }
    }

    public class SceneManager
    {
        private int UPDATEINTERVAL = 42;
        public delegate void PaintEventDelegate(DirectBitmap image);
        public DirectBitmap canvas;
        public object canvasLock { get; private set; }
        public List<Object3D> sceneObjects;
        public Dictionary<Object3D, List<Component>> componentInstances;

        private bool updating = false;
        private System.Timers.Timer updateTimer;
        

        public bool Started => updateTimer.Enabled;
        public Color worldColor = Color.NavajoWhite;
        public PaintEventDelegate PaintEvent;

        public SceneManager(object canvasLock)
        {
            updateTimer = new System.Timers.Timer(UPDATEINTERVAL);
            updateTimer.Elapsed += ((e, sender) => { if(!updating)UpdateScene(); });
            sceneObjects = new List<Object3D>();
            componentInstances = new Dictionary<Object3D, List<Component>>();
            this.canvasLock = canvasLock;
        }

        public void AddObject(Object3D obj)
        {
            sceneObjects.Add(obj);
            componentInstances.Add(obj, new List<Component>());
        }

        public void RemoveObject(Object3D obj)
        {
            sceneObjects.Remove(obj);
            componentInstances.Remove(obj);
        }

        public Component GetComponent(Object3D obj, Type type)
        {
            if (!Started)
                throw new Exception("GameEngine not running, instance does not exist yet");
            foreach(Component c in componentInstances[obj])
            {
                if(c.GetType() == type)
                {
                    return c;
                }
            }
            return null;
        }

        public void Start()
        {
            foreach (Object3D sceneObject in sceneObjects)
            {
                if (sceneObject.enabled)
                {
                    //Do requireComponent check on all objects and components
                    
                    foreach (Type c in sceneObject.components)
                    {
                        Attribute[] objectAttrs = Attribute.GetCustomAttributes(c);              
                        foreach (Attribute attr in objectAttrs)
                        {
                            Console.WriteLine(attr.ToString());
                            if (attr is RequireComponentAttribute)
                            {
                                if (!sceneObject.components.Contains(((RequireComponentAttribute)attr).component))
                                {
                                    throw new Exception((sceneObject.name+": Component "+ c+" requires component of type "+((RequireComponentAttribute)attr).component.ToString()));
                                }
                            }
                        }
                        Component compInstance = (Component)Activator.CreateInstance(c);
                        FieldInfo[] fi = sceneObject.componentFieldInfo[c];
                        for(int i = 0; i < fi.Length; i++)
                        {
                            fi[i].SetValue(compInstance, sceneObject.initialVals[c][i]);
                        }
                        compInstance.object3D = sceneObject;                        
                        componentInstances[sceneObject].Add(compInstance);
                    }
                }                
            }
            updateTimer.Start(); //Move this below
            foreach (Object3D sceneObject in sceneObjects)
            {
                if (sceneObject.enabled)
                {
                    foreach (Component c in componentInstances[sceneObject])
                    {
                        c.Start();
                    }
                }
            }
        }

        public void Stop()
        {
            updateTimer.Stop();
            foreach (List<Component> comps in componentInstances.Values)
            {
                comps.Clear();
            }
        }

        private void UpdateScene()
        {
            updating = true;
            foreach(Object3D obj in sceneObjects)
            {
                foreach(Component c in componentInstances[obj])
                {
                    c.Update();
                }
            }
            updating = false;
        }

    }

    public static class Input
    {
        private static Form appForm;
        private static bool initialized = false;

        private static bool[] keyDownList = new bool[Enum.GetNames(typeof(Keys)).Length];

        public static void SetForm(Form form)
        {
            if(appForm != null)
            {
                appForm.KeyUp -= AppForm_KeyUp;
                appForm.KeyDown -= AppForm_KeyDown;
            }
            appForm = form;
            initialized = true;
            form.KeyDown += AppForm_KeyDown;
            form.KeyUp += AppForm_KeyUp;
        }

        public static bool KeyHeld(Keys key)
        {
            if (!initialized)
                throw new Exception("Input source not set to a form");
            return keyDownList[(int)key];
        }

        private static void AppForm_KeyUp(object sender, KeyEventArgs e)
        {
            keyDownList[(int)e.KeyCode] = false;
        }

        private static void AppForm_KeyDown(object sender, KeyEventArgs e)
        {
            keyDownList[(int)e.KeyCode] = true;
        }
    }
}
