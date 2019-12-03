using System;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    class Driver
    {        
        [STAThread]
        static void Main(string[] args)
        {
            GameEngine engine = new GameEngine(512, 256);
            Thread.CurrentThread.Name = "Main";
            engine.Main();
            Application.Run(engine);   
        }        
    }

    class GameEngine : Form
    {

        public GameEngine(int windowWidth, int windowHeight)
        {
            FormClosed += (e, s) => { Environment.Exit(0); };
            Width = windowWidth;
            Height = windowHeight;
            GameManager.Initialize(windowWidth, windowHeight);
            Physics.Initialize();
            Time.OnPaintEvent = async (x) => { GameEngine_Paint2(); };
            DoubleBuffered = true;

            Input.SetForm(this);
        }

        public void Main()
        {
            Camera maincamera = new Camera(new Transform(new Vector3(0, -25, 0)), 512, 256, 0.7853f);
            maincamera.AddComponent(new CameraComponent());
            Object3D sphere = new Object3D(new Transform(new Vector3(0.5f, 0, 0)));
            sphere.name = "Sphere";
            sphere.AddComponent(new MeshComponent { mesh = new Mesh(@"..\..\Resources\Cylinder.obj") });
            sphere.AddComponent(new MyScript());
            sphere.AddComponent(new MeshRenderer { albedo = Color.Red });
            sphere.AddComponent(new MeshCollider());
            //sphere.AddComponent(new RigidBody());

            Object3D ground = new Object3D(new Transform(new Vector3(3, 0, 0)));
            ground.name = "Ground";
            ground.AddComponent(new MeshComponent { mesh = new Mesh(@"..\..\Resources\Cylinder.obj") });
            ground.AddComponent(new MeshRenderer { albedo = Color.Gray });
            ground.AddComponent(new MeshCollider());

            //sphere.AddComponent(new RigidBody());

            // Rig r = new Rig(@"\Users\Kirby\Desktop\CylinderRig.rig");
            // Console.WriteLine(r.root.name);

            GameManager.AddObject(maincamera);
            GameManager.AddObject(sphere);
            GameManager.AddObject(ground);
            //manager.AddObject(sphere2);

            Time.Start();
        }

        private void GameEngine_Paint2()
        {
            using(Graphics g = CreateGraphics())
            {
                g.DrawImage(GameManager.canvas.Bitmap, new Point(0, 0));
            }
        }

    }
}
