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
            Camera camera = new Camera(new Transform(new Vector3(0,-7, 0)), 512, 256, 0.155f);
            AppForm app = new AppForm(camera);
            
            camera.renderQueue.Add(new Object3D(new Transform(new Vector3(0, 0, -0.5f), 0.5f * Vector3.One), new Mesh(@"\Users\Kirby\Desktop\Eddie.obj")));
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

        private Bitmap cubeTexture;
        private int texWidth, texHeight;


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

            cubeTexture = new Bitmap(@"\Users\Kirby\Desktop\Eddie Texture.png");
            texWidth = cubeTexture.Width; texHeight = cubeTexture.Height;
        }

        private void AppForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                camera.transform.Location += Vector3.UnitVectorY;
            else if (e.KeyCode == Keys.Down)
                camera.transform.Location -= Vector3.UnitVectorY;
            else if (e.KeyCode == Keys.Space)
                camera.renderQueue[0].transform.Rotate(new Vector3(0, 0, 1), 0.075f);
            else if (e.KeyCode == Keys.A)
                camera.horizFOV -= 0.1f;
            else if (e.KeyCode == Keys.D)
                camera.horizFOV += 0.1f;
        }

        private void StartRender()
        {
                       
            camera.renderQueue[0].transform.Rotate(new Vector3(0, 0, 1), 0.025f);
            angle += 0.05f;
            if (!rendering) Invalidate();
        }

        Color[,] texture = new Color[2, 2] { { Color.Red, Color.Yellow }, { Color.Blue, Color.Green } };

        Color Interpolate(float x, float y)
        {
            byte a, r, g, b;

            a = (byte)(texture[0, 0].A * (1 - x) * (1 - y) + texture[1, 0].A * (x) * (1 - y) + texture[0, 1].A * (1 - x) * y + texture[1, 1].A * x * y);
            r = (byte)(texture[0, 0].R * (1 - x) * (1 - y) + texture[1, 0].R * (x) * (1 - y) + texture[0, 1].R * (1 - x) * y + texture[1, 1].R * x * y);
            g = (byte)(texture[0, 0].G * (1 - x) * (1 - y) + texture[1, 0].G * (x) * (1 - y) + texture[0, 1].G * (1 - x) * y + texture[1, 1].G * x * y);
            b = (byte)(texture[0, 0].B * (1 - x) * (1 - y) + texture[1, 0].B * (x) * (1 - y) + texture[0, 1].B * (1 - x) * y + texture[1, 1].B * x * y);
            return Color.FromArgb(a, r, g, b);
        }

        private Vector3 lampPos = new Vector3(1, -2, 2);

        private void SmoothShader(Fragment f)
        {
            float fac;
            fac = Vector3.Dot(f.normal, lampPos.Normalized);
            
            if (fac < 0) fac = 0;
 
            Color pix = cubeTexture.GetPixel((int)((texWidth - 1) * f.uv.x), (int)((texHeight - 1) * (1 - f.uv.y)));
            f.color = Color.FromArgb(pix.A, (byte)(fac * pix.R), (byte)(fac * pix.G), (byte)(fac * pix.B));
        }

        private void FogShader(Fragment f)
        {
            byte a, r = f.color.R, g = f.color.G, b = f.color.B;

            if(angle % 20 > 10)
            {
                f.color = Interpolate(f.uv.x, f.uv.y);
            }
            else
            {
                f.color = cubeTexture.GetPixel((int)(249 * f.uv.x), (int)(249 * f.uv.y));
            }

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
            camera.Render(image, SmoothShader, null);
            e.Graphics.DrawImage(image.Bitmap, new Point(0, 0));

            rendering = false;
        }
    }
}
