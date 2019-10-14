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
            engine.Main();
            Application.Run(engine);   
        }        
    }

    class GameEngine : Form
    {
        SceneManager manager;
        DirectBitmap image;
        object imageLock;

        public GameEngine(int windowWidth, int windowHeight)
        {
            Width = windowWidth;
            Height = windowHeight;
            imageLock = new object();
            manager = new SceneManager(imageLock);
            image = new DirectBitmap(1, 1);
            manager.PaintEvent = ((x) => { image = x; Invalidate(); });
            DoubleBuffered = true;

            Input.SetForm(this);
            Paint += GameEngine_Paint;
        }

        public void Main()
        {
            Camera maincamera = new Camera(manager, new Transform(new Vector3(0, -7, 0)), 512, 256, 0.7853f);
            maincamera.AddComponent(new CameraComponent());
            Object3D sphere = new Object3D(manager, new Transform(new Vector3(0.5f, 0, 0)));
            sphere.AddComponent(new MeshComponent { mesh = new Mesh(@"\Users\Kirby\Desktop\icosphere2.obj") });
            sphere.AddComponent(new MyScript());
            sphere.AddComponent(new MeshRenderer { albedo = Color.Red });


            manager.AddObject(maincamera);
            manager.AddObject(sphere);
            manager.Start();
        }

        private void GameEngine_KeyDown(object sender, KeyEventArgs e)
        { }

        private void GameEngine_Paint(object sender, PaintEventArgs e)
        {
            lock (imageLock)
            {
                e.Graphics.DrawImage(image.Bitmap, new Point(0, 0));
            }            
        }
    }
}
