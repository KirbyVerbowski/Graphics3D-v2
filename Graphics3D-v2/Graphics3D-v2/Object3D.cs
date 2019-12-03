using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Graphics3D_v2
{
    public enum ForceMode { Force, Acceleration, Velocity }

    public abstract class YieldInstruction
    {
        public Task beforeNext;
    }

    public class WaitForSeconds : YieldInstruction
    {
        public WaitForSeconds(float seconds)
        {
            beforeNext = Task.Delay((int)(seconds * 1000));
        }
    }
    public class WaitForFixedUpdate : YieldInstruction
    {
        AutoResetEvent EventWaitHandle = new AutoResetEvent(false);
        public WaitForFixedUpdate()
        {
            Time.OnFixedUpdateCoroutineEvent += EventListener;
            beforeNext = next();
        }
        private Task next()
        {
            EventWaitHandle.WaitOne();
            Time.OnFixedUpdateCoroutineEvent -= EventListener;
            return Task.CompletedTask;
        }
        private Task EventListener()
        {
            EventWaitHandle.Set();
            return Task.CompletedTask;
        }
    }
    public class WaitForUpdate : YieldInstruction
    {
        AutoResetEvent EventWaitHandle = new AutoResetEvent(false);
        public WaitForUpdate()
        {
            Time.OnFixedUpdateCoroutineEvent += EventListener;
            beforeNext = next();
        }
        private Task next()
        {
            EventWaitHandle.WaitOne();
            Time.OnFixedUpdateCoroutineEvent -= EventListener;
            return Task.CompletedTask;
        }
        private Task EventListener()
        {
            EventWaitHandle.Set();
            return Task.CompletedTask;
        }
    }

    public class Object3D
    {
        public List<Type> components = new List<Type>();
        public Dictionary<Type, FieldInfo[]> componentFieldInfo = new Dictionary<Type, FieldInfo[]>();
        public Dictionary<Type, object[]> initialVals = new Dictionary<Type, object[]>();

        public bool enabled = true; //Will this object be affected by scenemanager (start / update)
        public bool visible = true;
        public Transform transform;
        public string name = "unnamed object";

        public Object3D()
        {
            this.transform = new Transform();
        }
        public Object3D(Transform transform)
        {
            this.transform = transform;
        }
        public Object3D(Transform transform, string name)
        {
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
            return GameManager.GetComponent(this, component);
        }
    }

    public static class GameManager
    {
        public static List<Object3D> sceneObjects;
        public static Dictionary<Object3D, List<Component>> componentInstances;

        public static Color worldColor = Color.NavajoWhite;
        public static DirectBitmap canvas;

        private static bool _initialized = false;
        public static void Initialize(int WindowWidth, int WindowHeight)
        {
            canvas = new DirectBitmap(WindowWidth, WindowHeight);
            Time.OnInitEvent += OnInit;
            Time.OnAwakeEvent += OnAwake;
            Time.OnStartEvent += OnStart;
            Time.OnStopEvent += OnStop;
            Time.OnUpdateEvent += OnUpdate;
            Time.OnFixedUpdateEvent += OnFixedUpdate;
            Time.OnPreRenderEvent += OnPreRender;
            Time.OnPostRenderEvent += OnPostRender;
            sceneObjects = new List<Object3D>();
            componentInstances = new Dictionary<Object3D, List<Component>>();
            _initialized = true;
        }

        public static void AddObject(Object3D obj)
        {
            if (!_initialized)
                throw new Exception("GameManager not initialized");
            sceneObjects.Add(obj);
            componentInstances.Add(obj, new List<Component>());
        }

        public static void RemoveObject(Object3D obj)
        {
            if (!_initialized)
                throw new Exception("GameManager not initialized");
            sceneObjects.Remove(obj);
            componentInstances.Remove(obj);
        }

        public static Component GetComponent(Object3D obj, Type type)
        {
            if (!_initialized)
                throw new Exception("GameManager not initialized");
            foreach (Component c in componentInstances[obj])
            {
                if(type.IsAssignableFrom(c.GetType()))
                {
                    return c;
                }
            }
            return null;
        }

        public static List<Component> GetAllComponents(Type type)
        {
            List<Component> instanceList = new List<Component>();
            foreach(Object3D obj in sceneObjects)
            {
                foreach(Component c in componentInstances[obj])
                {
                    if (type.IsAssignableFrom(c.GetType()))
                    {
                        instanceList.Add(c);
                    }
                }
            }
            return instanceList;
        }

        private static Task OnInit()
        {
            foreach (Object3D sceneObject in sceneObjects)
            {
                if (sceneObject.enabled)
                {
                    //Do requireComponent check on all objects
                    Attribute[] objectAttrs = Attribute.GetCustomAttributes(sceneObject.GetType());
                    foreach (Attribute attr in objectAttrs)
                    {
                        if (attr is RequireComponentAttribute)
                        {
                            if (!sceneObject.components.Contains(((RequireComponentAttribute)attr).component))
                            {
                                throw new Exception((sceneObject.name + ": Requires component of type " + ((RequireComponentAttribute)attr).component.ToString()));
                            }
                        }
                    }
                    //Do requireComponent check on all components                    
                    foreach (Type c in sceneObject.components)
                    {
                        Attribute[] componentAttrs = Attribute.GetCustomAttributes(c);
                        foreach (Attribute attr in componentAttrs)
                        {
                            if (attr is RequireComponentAttribute)
                            {
                                bool hasIt = false;
                                foreach(Type objComponent in sceneObject.components)
                                {
                                    if (((RequireComponentAttribute)attr).component.IsAssignableFrom(objComponent))
                                        hasIt = true;
                                }
                                
                                if (!hasIt)
                                {
                                    throw new Exception((sceneObject.name + ": Component " + c + " requires component of type " + ((RequireComponentAttribute)attr).component.ToString()));
                                }
                            }
                        }
                        //Create instances of all components and restore their original field values
                        Component compInstance = (Component)Activator.CreateInstance(c);
                        FieldInfo[] fi = sceneObject.componentFieldInfo[c];
                        for (int i = 0; i < fi.Length; i++)
                        {
                            fi[i].SetValue(compInstance, sceneObject.initialVals[c][i]);
                        }
                        compInstance.object3D = sceneObject;
                        componentInstances[sceneObject].Add(compInstance);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static Task OnAwake()
        {
            foreach (Object3D sceneObject in sceneObjects)
            {
                if (sceneObject.enabled)
                {
                    foreach (Component c in componentInstances[sceneObject])
                    {
                        c.Awake();
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static Task OnStart()
        {       
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
            return Task.CompletedTask;
        }

        public static Task OnStop()
        {
            foreach (List<Component> comps in componentInstances.Values)
            {
                comps.Clear();
            }
            return Task.CompletedTask;
        }

        private static Task OnUpdate()
        {
            foreach(Object3D obj in sceneObjects)
            {
                foreach(Component c in componentInstances[obj])
                {
                    c.Update();
                }
            }
            return Task.CompletedTask;
        }

        private static Task OnFixedUpdate()
        {
            foreach (Object3D obj in sceneObjects)
            {
                foreach (Component c in componentInstances[obj])
                {
                    c.FixedUpdate();
                }
            }
            return Task.CompletedTask;
        }

        private static Task<DirectBitmap> OnPreRender(DirectBitmap b)
        {
            foreach (Object3D obj in sceneObjects)
            {
                foreach (Component c in componentInstances[obj])
                {
                    b = c.OnPreRender(b);
                }
            }
            return Task.FromResult(b);
        }

        private static Task<DirectBitmap> OnPostRender(DirectBitmap b)
        {
            foreach (Object3D obj in sceneObjects)
            {
                foreach (Component c in componentInstances[obj])
                {
                    b = c.OnPostRender(b);
                }
            }
            return Task.FromResult(b);
        }

    }

    public static class Input
    {
        private static Form appForm;
        private static bool _initialized = false;

        private static bool[] keyDownList = new bool[Enum.GetNames(typeof(Keys)).Length];
        private static bool[] mouseDownList = new bool[Enum.GetNames(typeof(MouseButtons)).Length];

        public static Vector2 mousePos = Vector2.Zero;


        public static void SetForm(Form form)
        {
            if(appForm != null)
            {
                appForm.KeyUp -= AppForm_KeyUp;
                appForm.KeyDown -= AppForm_KeyDown;
            }
            appForm = form;
            _initialized = true;
            form.KeyDown += AppForm_KeyDown;
            form.KeyUp += AppForm_KeyUp;

            form.MouseMove += Form_MouseMove;
            form.MouseDown += Form_MouseDown;
            form.MouseUp += Form_MouseUp;
            
        }

        public static bool MouseButtonDown(MouseButtons button)
        {
            if (!_initialized)
                throw new Exception("Input source not set to a form");
            return mouseDownList[(int)button];
        }

        public static bool KeyHeld(Keys key)
        {
            if (!_initialized)
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

        private static void Form_MouseUp(object sender, MouseEventArgs e)
        {
            keyDownList[(int)e.Button] = false;
        }

        private static void Form_MouseDown(object sender, MouseEventArgs e)
        {
            keyDownList[(int)e.Button] = true;
        }

        private static void Form_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos.x = e.X; mousePos.y = e.Y;
        }
    }

    public static class Time
    {

        static Stopwatch timer;
        //In ticks
        static double FRAME_CAP => ((double)MIN_FRAME_TIME_MILLI / 1000) * Stopwatch.Frequency;
        static double PHYSICS_CAP => ((double)MIN_PHYSICS_STEP / 1000) * Stopwatch.Frequency;
        static long timeAfterRender = 0;
        static long deltaTicks;
        static long fixedDeltaTicks;
        static long timeAfterPhysicsUpdate = 0;
        static bool _paused = false;

        static Task RenderLoopTask;
        static Task PhysicsLoopTask;

        public static Func<DirectBitmap, Task<DirectBitmap>> OnPreRenderEvent;
        public static Func<DirectBitmap, Task<DirectBitmap>> OnPostRenderEvent;
        public static Func<DirectBitmap, Task> OnPaintEvent;
        public static Func<Task> OnInitEvent;
        public static Func<Task> OnAwakeEvent;
        public static Func<Task> OnStartEvent;
        public static Func<Task> OnStopEvent;
        public static Func<Task> OnUpdateEvent;
        public static Func<Task> OnFixedUpdateEvent;
        public static Func<Task> OnFixedUpdateCoroutineEvent;
        public static Func<Task> OnUpdateCoroutineEvent;
        public static Func<Task> OnInternalPhysicsEvent;

        public static long MIN_PHYSICS_STEP = 42;
        public static long MIN_FRAME_TIME_MILLI = 42;//42
        //Time in seconds since start
        public static double time => ((double)Stopwatch.Frequency / (double)timer.ElapsedTicks);
        //Time in seconds last frame to complete
        public static double deltaTime => ((double)deltaTicks / (double)Stopwatch.Frequency);
        //Time in seconds last physics step to complete
        public static double fixedDeltaTime => ((double)fixedDeltaTicks / (double)Stopwatch.Frequency);

        public static void Start()
        {
            timer = new Stopwatch();
            timer.Start();

            //Do like this so that if any of the message threads get an exception (like in a user script)
            //The exception is propogated to the main thread and terminates the program
            Task InitTask = OnInitEvent();
            InitTask.ContinueWith(ThreadExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            InitTask.Wait();
            Task AwakeTask = OnAwakeEvent();
            AwakeTask.ContinueWith(ThreadExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            AwakeTask.Wait();
            Task StartTask = OnStartEvent();
            StartTask.ContinueWith(ThreadExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            StartTask.Wait();

            RenderLoopTask = RenderLoop();
            RenderLoopTask.ContinueWith(ThreadExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);

            PhysicsLoopTask = PhysicsLoop();
            PhysicsLoopTask.ContinueWith(ThreadExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Pause()
        {
            _paused = true;
        }

        public static void Resume()
        {
            _paused = false;
        }

        public static void Stop()
        {
            timer.Stop();
            timer.Reset();
            try
            {
                if (RenderLoopTask.Status != TaskStatus.Faulted)
                    RenderLoopTask.Wait();
                if(PhysicsLoopTask.Status != TaskStatus.Faulted)
                    PhysicsLoopTask.Wait();
            }
            catch (NullReferenceException){ /*Thread wasn't started, nothing to wait for*/ }

            OnStopEvent();
        }

        private static async Task RenderLoop()
        {            
            while (true)
            {
                Task RenderOps = new Task(async () =>
                {
                    await OnUpdateEvent();
                    if (OnUpdateCoroutineEvent != null)
                        await OnUpdateCoroutineEvent();
                    GameManager.canvas = await OnPreRenderEvent(GameManager.canvas);
                    GameManager.canvas = await OnPostRenderEvent(GameManager.canvas);
                    await OnPaintEvent(GameManager.canvas);
                });
                RenderOps.Start();
                await Task.WhenAll(Task.Delay((int)MIN_FRAME_TIME_MILLI), RenderOps);

                deltaTicks = timer.ElapsedTicks - timeAfterRender;
                timeAfterRender = timer.ElapsedTicks;

                if (!timer.IsRunning)
                    break;
            }
        }

        private static async Task PhysicsLoop()
        {
            while (true)
            {
                Task PhysicsOps = new Task(async () =>
                {
                    await OnFixedUpdateEvent();
                    await OnInternalPhysicsEvent();
                    if (OnFixedUpdateCoroutineEvent != null)
                        await OnFixedUpdateCoroutineEvent();
                });
                PhysicsOps.Start();
                await Task.WhenAll(Task.Delay((int)MIN_PHYSICS_STEP), PhysicsOps);

                fixedDeltaTicks = timer.ElapsedTicks - timeAfterPhysicsUpdate;
                timeAfterPhysicsUpdate = timer.ElapsedTicks;

                if (!timer.IsRunning)
                    break;
            }
        }

        private static void ThreadExceptionHandler(Task t)
        {
            
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("   ");
            
            string[] StackTrace = r.Split(t.Exception.InnerException.StackTrace);
            System.Text.StringBuilder newStackTrace = new System.Text.StringBuilder();
            newStackTrace.Append("Fatal error: " + t.Exception.InnerException.GetType().ToString() + ": " +t.Exception.InnerException.Message + "\r\n");
            for(int i = 0; i < StackTrace.Length - 1; i++)
            {
                newStackTrace.Append("   " + StackTrace[i]);
            }
            Console.WriteLine(newStackTrace.ToString());
            Stop();
            Console.WriteLine("Game stopped");
        }
    }

    public static class Physics
    {

        private static bool _initialized = false;
        public static void Initialize()
        {
            Time.OnInternalPhysicsEvent += InternalPhysicsUpdate;
            _initialized = true;
        }

        public static float gravity = 9.81f;

        private static Task InternalPhysicsUpdate()
        {
            if (!_initialized)
                throw new Exception("Physics engine not initialized");

            Collider objCollider;
            Vector3 nextPos = Vector3.Zero;
            float distanceToColliderFront = 0;


            foreach (RigidBody r in GameManager.GetAllComponents(typeof(RigidBody)))
            {
                if (!r.Static)
                {
                    bool doCollision = (r.velocity != Vector3.Zero);

                    objCollider = (Collider)r.object3D.GetComponent(typeof(Collider));
                    Console.WriteLine(objCollider.GetType());
                    nextPos = r.object3D.transform.Location + r.velocity * (float)Time.fixedDeltaTime;

                    if(doCollision)
                        distanceToColliderFront = objCollider.DistanceToColliderSurface(r.velocity);
                    //Do Collision detection based on next position

                    if (doCollision && RayCast(new Ray(r.object3D.transform.Location + r.velocity.Normalized * distanceToColliderFront, r.velocity), out RayCastHit hit, objCollider))
                    {
                        r.object3D.transform.Location += r.velocity.Normalized * (hit.distance - distanceToColliderFront - 0.0001f);
                        r.velocity = Vector3.Zero;
                        r.netForce = Vector3.Zero;
                        Console.WriteLine("Collision");
                        r.Static = true;
                    }
                    else
                    {
                        //Move object
                        r.object3D.transform.Location = nextPos;
                        //Apply gravity
                        r.netForce += r.mass * -Vector3.UnitVectorZ * gravity;
                        //Update velocity vector
                        r.velocity += r.Acceleration * (float)Time.fixedDeltaTime;
                    }

                    
                }
                
            }

            return Task.CompletedTask;
        }

        public static bool RayCast(Ray ray, out RayCastHit hit)
        {
            if(RayCastAll(ray, out RayCastHit[] hits))
            {
                float minDist = float.PositiveInfinity;
                RayCastHit minhit = null;
                foreach(RayCastHit h in hits)
                {
                    if(h.distance < minDist)
                    {
                        minDist = h.distance;
                        minhit = h;
                    }
                }
                hit = minhit;
                return true;
            }
            else
            {
                hit = null;
                return false;
            }
        }

        public static bool RayCast(Ray ray, out RayCastHit hit, Collider ignoreCollider)
        {
            if (RayCastAll(ray, out RayCastHit[] hits, ignoreCollider))
            {
                float minDist = float.PositiveInfinity;
                RayCastHit minhit = null;
                foreach (RayCastHit h in hits)
                {
                    if (h.distance < minDist)
                    {
                        minDist = h.distance;
                        minhit = h;
                    }
                }
                hit = minhit;
                return true;
            }
            else
            {
                hit = null;
                return false;
            }
        }

        public static bool RayCastAll(Ray ray, out RayCastHit[] hits)
        {
            bool hitResult = false;
            List<RayCastHit> hitList = new List<RayCastHit>();
            foreach(MeshCollider collider in GameManager.GetAllComponents(typeof(MeshCollider)))
            {
                if(collider.RayCast(ray, out RayCastHit hit))
                {
                    hitList.Add(hit);
                    hitResult = true;
                }
            }
            hits = hitList.ToArray();
            return hitResult;
        }

        public static bool RayCastAll(Ray ray, out RayCastHit[] hits, Collider ignoreCollider)
        {
            bool hitResult = false;
            List<RayCastHit> hitList = new List<RayCastHit>();
            foreach (Collider collider in GameManager.GetAllComponents(typeof(Collider)))
            {
                if (collider == ignoreCollider)
                    continue;
                if (collider.RayCast(ray, out RayCastHit hit))
                {
                    hitList.Add(hit);
                    hitResult = true;
                }
            }
            hits = hitList.ToArray();
            return hitResult;
        }
    }


}
