using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Graphics3D_v2
{

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Clear()
        {
            Array.Clear(Bits, 0, Bits.Length);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }

    class Driver
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            Camera camera = new Camera(new Transform(), 512, 256, 0.8552f);

            camera.renderQueue.Add(new Object3D(new Transform(new Vector3(0, 10f, 0)), new Mesh(@"..\..\Resources\Cube.obj")));
            AppForm app = new AppForm(camera);
            Application.Run(app);
           
        }
    }


    class AppForm : Form
    {
        public Camera camera;
        public List<Object3D> sceneObjects;
        public System.Windows.Forms.Timer renderTimer;
        private bool rendering = false;
        float angle = 0;
        DirectBitmap image;

        public AppForm(Camera c)
        {
            Width = 512;
            Height = 256;
            Paint += AppForm_Paint;
            camera = c;
            sceneObjects = new List<Object3D>();
            renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 4;
            renderTimer.Start();
            renderTimer.Tick += ((sender, e) => StartRender());
            DoubleBuffered = true;
            image = new DirectBitmap(Width, Height);

        }

        private void StartRender()
        {
            camera.renderQueue[0].transform.Location = new Vector3(2.5f * (float)Math.Sin(angle), 10, 2.5f * (float)Math.Cos(angle));
            camera.renderQueue[0].transform.Rotate(new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), (float)Math.Sin(angle)), 0.05f);
            angle += 0.05f;
            if (!rendering) Invalidate();
        }

        private void AppForm_Paint(object sender, PaintEventArgs e)
        {
            
            rendering = true;
            image.Clear();
            camera.Render(image);
            e.Graphics.DrawImage(image.Bitmap, new Point(0, 0));

            rendering = false;
        }
    }
}
