using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Graphics3D_v2
{
    
    class Driver
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            Camera camera = new Camera(new Transform(), 512, 256, 0.8552f);

            camera.renderQueue.Add(new Object3D(new Transform(new Vector3(0, 2.5f, 0)), new Mesh(@"..\..\Resources\Cube.obj")));
            AppForm app = new AppForm(camera);
            Application.Run(app);
        }
    }


    class AppForm : Form
    {
        public Camera camera;
        public List<Object3D> sceneObjects;
        public Timer renderTimer;

        public AppForm(Camera c)
        {
            Width = 512;
            Height = 256;
            Paint += AppForm_Paint;
            camera = c;
            sceneObjects = new List<Object3D>();
            renderTimer = new Timer();
            renderTimer.Interval = 2;
            renderTimer.Start();
            renderTimer.Tick += ((sender, e) => Invalidate());
        }

        private void AppForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(1, -1);
            e.Graphics.TranslateTransform(Width / 2, -Height / 2);
            camera.Render(e.Graphics);
        }
    }
}
