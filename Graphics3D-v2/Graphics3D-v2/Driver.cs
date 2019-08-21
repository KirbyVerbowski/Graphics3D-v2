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
            Camera camera = new Camera(new Transform(new Vector3(0,-2, 0)), 512, 256, 0.8552f);
            AppForm app = new AppForm(camera);
            
            camera.renderQueue.Add(new Object3D(new Transform(new Vector3(0, 0, 0)), new Mesh(@"\Users\Kirby\Desktop\icosphere.obj")));
            Application.Run(app);            
        }
    }


    class AppForm : Form
    {
        public Camera camera;
        public List<Object3D> sceneObjects;
        public System.Windows.Forms.Timer renderTimer;
        public System.Diagnostics.Stopwatch appTimer;
        private bool rendering = false;
        float angle = 0;
        public DirectBitmap image;


        public AppForm(Camera c)
        {
            Width = 512;
            Height = 256;
            Paint += AppForm_Paint;
            camera = c;
            sceneObjects = new List<Object3D>();
            renderTimer = new System.Windows.Forms.Timer();
            appTimer = new System.Diagnostics.Stopwatch();
            appTimer.Start();
            renderTimer.Interval = 4;
            renderTimer.Start();
            renderTimer.Tick += ((sender, e) => StartRender());
            DoubleBuffered = true;
            image = new DirectBitmap(Width, Height);

            KeyDown += AppForm_KeyDown;
        }

        private void AppForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                camera.transform.Location += Vector3.UnitVectorY;
            else if (e.KeyCode == Keys.Down)
                camera.transform.Location -= Vector3.UnitVectorY;
        }

        private void StartRender()
        {

            //camera.renderQueue[0].transform.Location = new Vector3(2 * (float)Math.Cos(angle), 15, 2 * (float)Math.Sin(angle) + 0.5f);
            //camera.transform.Location = new Vector3(5 * (float)Math.Sin(angle), 0, 0);
            camera.renderQueue[0].transform.Rotate(new Vector3(0, 0, 1), 0.025f);
            angle += 0.05f;
            if (!rendering) Invalidate();
        }

        private void FogShader(Fragment f)
        {
            byte a, r, g, b;

            r = (byte)(Math.Abs(Vector3.Dot(f.normal, Vector3.UnitVectorZ)) * 255);
            g = (byte)(Math.Abs(Vector3.Dot(f.normal, Vector3.UnitVectorX)) * 255);
            b = (byte)(Math.Abs(Vector3.Dot(f.normal, Vector3.UnitVectorY)) * 255);

            float fac = 1 - (f.z / 3f);
            if (fac < 0) fac = 0;
            
            a = (byte)(f.color.A * fac);
            r = (byte)(r * fac);
            g = (byte)(g * fac);
            b = (byte)(b * fac);
            f.color = Color.FromArgb(a, r, g, b);
        }

        private void SquishShader(Vertex v)
        {
            float total = v.localPos.x + v.localPos.y + v.localPos.z;
            float fac = (float)((Math.Sin(angle + total) + 2) / 2);

            v.localPos *= fac;
        }

        private void AppForm_Paint(object sender, PaintEventArgs e)
        {
            
            rendering = true;
            image.Clear();
            camera.Render(image, FogShader, SquishShader);
            //Camera.Triangle t = new Camera.Triangle(new Vector2(50.4f, 60f), new Vector2(250.4f, 120f), new Vector2(100.234f, 135f), 20, 50, 90);
            //camera.DrawTriangle(t, image, new float[5], Color.Red);
            e.Graphics.DrawImage(image.Bitmap, new Point(0, 0));

            rendering = false;
        }
    }
}
