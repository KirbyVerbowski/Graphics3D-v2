/*using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Graphics3D_v2
{

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public BitmapData bitmapData;
        public Int32[] Pixels;

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            Pixels = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void LockBits()
        {
            bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, Bitmap.PixelFormat);


            Array.Clear(Pixels, 0, Pixels.Length);
            Marshal.Copy(BitsHandle.AddrOfPinnedObject(), Pixels, 0, Pixels.Length);
        }

        public void UnlockBits()
        {
            Marshal.Copy(Pixels, 0, BitsHandle.AddrOfPinnedObject(), Pixels.Length);
            Bitmap.UnlockBits(bitmapData);
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Pixels[index] = col;
            
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Pixels[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void ClearColor(Int32 color)
        {
            for(int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = color;
                Pixels[i] = color;
            }
        }

        public void Clear()
        {
            Array.Clear(Bits, 0, Bits.Length);
            Array.Clear(Pixels, 0, Pixels.Length);
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
            //Fix camera width instead of projection distance
            Camera camera = new Camera(new Transform(new Vector3(0,-7, 0)), 512, 256, 0.7853f);
            AppForm app = new AppForm(camera);

            Object3D obj = new Object3D(new Transform(new Vector3(0, 0, 0)), new Mesh(@"\Users\Kirby\Desktop\sculpt.obj", null, app.TextureShader));
            Bitmap tex = new Bitmap(@"C:\Users\Kirby\Desktop\Eddie texture.png");
            obj.mesh.shaderParams.Add(tex);
            //obj.mesh.shaderParams.Add(Color.Blue);

            camera.renderQueue.Add(obj);
            Application.Run(app);            
        }

        
    }


    class AppForm : Form
    {
        public Camera camera;
        public List<Object3D> sceneObjects;
        public System.Windows.Forms.Timer renderTimer;
        public System.Diagnostics.Stopwatch appTimer;
        long frameTime;
        int framerate;
        float angle = 0;
        public DirectBitmap image;

        //Color[,] texture = new Color[2, 2] { { Color.HotPink, Color.LightSkyBlue }, { Color.White, Color.LightSeaGreen } };
        private Vector3 lampPos = new Vector3(1, -2, 2);
    
        public AppForm(Camera c)
        {

            Width = 512;
            Height = 256;
            Paint += AppForm_Paint;
            camera = c;
            sceneObjects = c.renderQueue;
            renderTimer = new System.Windows.Forms.Timer();
            appTimer = new System.Diagnostics.Stopwatch();
            appTimer.Start();
            //25FPS
            renderTimer.Interval = 42;
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
            else if (e.KeyCode == Keys.Right)
                camera.transform.Location += Vector3.UnitVectorX;
            else if (e.KeyCode == Keys.Left)
                camera.transform.Location -= Vector3.UnitVectorX;
            else if (e.KeyCode == Keys.Space)
                camera.renderQueue[0].transform.Rotate(new Vector3(0, 0, 1), 0.075f);
            else if (e.KeyCode == Keys.A)
                camera.transform.Rotate(Vector3.UnitVectorZ, 0.05f);
            else if (e.KeyCode == Keys.D)
                camera.transform.Rotate(Vector3.UnitVectorZ, -0.05f);
            else if (e.KeyCode == Keys.W)
                camera.transform.Rotate(Vector3.UnitVectorX, 0.05f);
            else if (e.KeyCode == Keys.S)
                camera.transform.Rotate(Vector3.UnitVectorX, -0.05f);
            Quaternion q1 = new Quaternion();
            Quaternion q2 = new Quaternion();
            Quaternion q3 = q1 * q2;
        }

        private void StartRender()
        {
            appTimer.Start();

            lampPos = Vector3.Rotate(lampPos, Vector3.UnitVectorY, 0.15f);
            angle += 0.05f;
            Invalidate();
        }

        public void ToonShader(Fragment f)
        { //Expects albedo color to be in shaderparams[0]
            Color albedo = (Color)f.thisMesh.shaderParams[0];
            float thresh1 = 0.8f, thresh2 = 0.5f, middleFac = 0.4f;
            float fac = Vector3.Dot(lampPos.Normalized, f.normal);
            float specPow = 20;
            float specK = 1;
            //Add a method to retrieve nearest vertex info / triangle in global space
            Vector3 Rm = 2 * Vector3.Dot((lampPos - f.thisMesh.vertices[0]).Normalized, f.normal) * f.normal - (lampPos - f.thisMesh.vertices[0]).Normalized;
            Vector3 V = (f.camera.transform.Location - f.thisMesh.vertices[0]).Normalized;
            float dot = (specK * Vector3.Dot(Rm, V));
            double R = Math.Pow(dot, specPow);
            if(R > 0.65)
                Console.WriteLine(R);
            if (fac < 0)
                fac = 0;
            if(fac > 0.97f)
            {
                f.color = Color.White;
                //float r = albedo.R / middleFac > 255 ? albedo.R / middleFac : 255;
                //f.color = Color.FromArgb((int)(albedo.R / middleFac > 255 ? albedo.R / middleFac : 255), (int)(albedo.G / middleFac > 255 ? albedo.R / middleFac : 255), (int)(albedo.R / middleFac > 255 ? albedo.B / middleFac : 255));
            }
            else if(fac > thresh1)
            {
                f.color = albedo;
            }else 
            {
                f.color = Color.FromArgb((int)(albedo.R * middleFac), (int)(albedo.G * middleFac), (int)(albedo.B * middleFac));
            }


            if (Vector3.Dot(-f.camera.transform.Forward, f.normal) < 0.5f)
                f.color = Color.Black;
        }

        /*Color Interpolate(float x, float y)
        {
            byte a, r, g, b;

            a = (byte)(texture[0, 0].A * (1 - x) * (1 - y) + texture[1, 0].A * (x) * (1 - y) + texture[0, 1].A * (1 - x) * y + texture[1, 1].A * x * y);
            r = (byte)(texture[0, 0].R * (1 - x) * (1 - y) + texture[1, 0].R * (x) * (1 - y) + texture[0, 1].R * (1 - x) * y + texture[1, 1].R * x * y);
            g = (byte)(texture[0, 0].G * (1 - x) * (1 - y) + texture[1, 0].G * (x) * (1 - y) + texture[0, 1].G * (1 - x) * y + texture[1, 1].G * x * y);
            b = (byte)(texture[0, 0].B * (1 - x) * (1 - y) + texture[1, 0].B * (x) * (1 - y) + texture[0, 1].B * (1 - x) * y + texture[1, 1].B * x * y);
            return Color.FromArgb(a, r, g, b);
        }*/

/*public void MirrorShader(Fragment f)
{
    float fac = 1f;
    Color pix;

    pix = mirrorTexture.GetPixel((int)((f.x)), (int)(f.y));
    f.color = Color.FromArgb(pix.A, (byte)(fac * pix.R), (byte)(fac * pix.G), (byte)(fac * pix.B));

}*/
/*
        public void TextureShader(Fragment f)
        {
            float fac;
            fac = Vector3.Dot(f.normal, lampPos.Normalized);
            
            if (fac < 0) fac = 0;
            Color pix;
            Bitmap tex = (Bitmap)f.thisMesh.shaderParams[0];
            pix = tex.GetPixel((int)((tex.Width - 1) * f.uv.x), (int)((tex.Height - 1) * (1 - f.uv.y)));
            f.color = Color.FromArgb(pix.A, (byte)(fac * pix.R), (byte)(fac * pix.G), (byte)(fac * pix.B));
        }

        private void SquishShader(Vertex v)
        {
            float total = v.location.x + v.location.y + v.location.z;
            float fac = (float)((Math.Sin(angle + total) + 2) / 2);

            v.location *= fac;
        }

        private void AppForm_Paint(object sender, PaintEventArgs e)
        {

            image.LockBits();
            image.Clear();
            camera.Render(image);
            image.UnlockBits();

            e.Graphics.DrawImage(image.Bitmap, new Point(0, 0));
            

            frameTime = appTimer.ElapsedMilliseconds;
            if (frameTime != 0)
                framerate = 1000 / (int)frameTime;
            e.Graphics.DrawString(framerate.ToString(), SystemFonts.DefaultFont, SystemBrushes.InfoText, new PointF(0, 0));
            

            appTimer.Reset();
        }
    }
}
*/


//Some old camera code

/*
                    drawPoints.Clear(); //Will be a poygon at most 6 sides
                    bool added1 = false, added2 = false, added3 = false;

                    if (NearFarClip(ref e1, ref e2, out bool moved1nf, out bool moved2nf))
                    {
                        if (EdgeClip(ref e1, ref e2, out bool moved1ec, out bool moved2ec))
                        {
                            if (moved1ec || moved1nf)
                            {
                                drawPoints.Add(e1);
                            }
                            else if (!added1)
                            {
                                drawPoints.Add(e1); added1 = true;
                            }
                            if (moved2ec || moved2nf)
                            {
                                drawPoints.Add(e2);
                            }
                            else if (!added2)
                            {
                                drawPoints.Add(e2); added2 = true;
                            }
                        }
                    }
                    e1 = beforeClip1; e2 = beforeClip2;
                    if (NearFarClip(ref e2, ref e3, out moved2nf, out bool moved3nf))
                    {
                        if (EdgeClip(ref e2, ref e3, out bool moved2ec, out bool moved3ec))
                        {
                            if (moved2ec || moved2nf)
                            {
                                drawPoints.Add(e2);
                            }
                            else if (!added2)
                            {
                                drawPoints.Add(e2); added2 = true;
                            }
                            if (moved3ec || moved3nf)
                            {
                                drawPoints.Add(e3);
                            }
                            else if (!added3)
                            {
                                drawPoints.Add(e3); added3 = true;
                            }
                        }
                    }
                    e2 = beforeClip2; e3 = beforeClip3;
                    if (NearFarClip(ref e3, ref e1, out moved3nf, out moved1nf))
                    {
                        if (EdgeClip(ref e3, ref e1, out bool moved3ec, out bool moved1ec))
                        {
                            if (moved3ec || moved3nf)
                            {
                                drawPoints.Add(e3);
                            }
                            else if (!added3)
                            {
                                drawPoints.Add(e3); added3 = true;
                            }
                            if (moved1ec || moved1nf)
                            {
                                drawPoints.Add(e1);
                            }
                            else if (!added1)
                            {
                                drawPoints.Add(e1); added1 = true;
                            }
                        }
                    }

                    if (drawPoints.Count > 3)
                    {
                        tris = Triangulate(drawPoints.ToArray(), projectedTriangle);
                        foreach (Triangle subtri in tris)
                        {
                            //Find clipped n-gon in camera local space -> triangulate -> project triangles -> draw
                            //instead of n-gon -> project -> triangulate -> draw
                            //this still isn't right
                            subtri.v0 = new Vector2(renderWidth / 2f + (subtri.v0.x * normToScreen.x), (renderHeight / 2f + (subtri.v0.y * normToScreen.z)));
                            subtri.v1 = new Vector2(renderWidth / 2f + (subtri.v1.x * normToScreen.x), (renderHeight / 2f + (subtri.v1.y * normToScreen.z)));
                            subtri.v2 = new Vector2(renderWidth / 2f + (subtri.v2.x * normToScreen.x), (renderHeight / 2f + (subtri.v2.y * normToScreen.z)));

                            DrawTriangle(subtri, b, depthBuffer, mesh.faceNormals[face], fShader);
                            
                        }
                        
                    }
                    else if (drawPoints.Count == 3)
                    {
                        for (int i = 0; i < drawPoints.Count; i++)
                        {
                            drawPoints[i] = new Vector3(renderWidth / 2f + (drawPoints[i].x * normToScreen.x), drawPoints[i].y, (renderHeight / 2f + (drawPoints[i].z * normToScreen.z)));
                        }

                        tri.v0 = new Vector2(drawPoints[0].x, drawPoints[0].z); tri.v1 = new Vector2(drawPoints[1].x, drawPoints[1].z); tri.v2 = new Vector2(drawPoints[2].x, drawPoints[2].z);
                        tri.z0 = e1.y; tri.z1 = e2.y; tri.z2 = e3.y;
                        tri.uv0 = mesh.uvcoords[mesh.uvs[face, 0]]; tri.uv1 = mesh.uvcoords[mesh.uvs[face, 1]]; tri.uv2 = mesh.uvcoords[mesh.uvs[face, 2]];
                        tri.vn0 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 0]]; tri.vn1 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 1]]; tri.vn2 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 2]];
                        DrawTriangle(tri, b, depthBuffer, mesh.faceNormals[face], fShader);
                        
                    }
                    */



//Takes a convex, counterclockwise winding n-gon and returns n - 2 list of triangles (fan method)
/*
private Triangle[] Triangulate(Vector3[] polygon, Triangle originalTriangle)
{
    Vector3 center = Vector3.Zero;

    foreach (Vector3 vert in polygon)
        center += vert;
    center *= 1 / (float)polygon.Length;
    center.y = 0;
    Vector3 v0 = new Vector3(polygon[0].x - center.x, 0, polygon[0].z - center.z);
    Array.Sort(polygon, ((a, b) =>
    {
        double theta1 = Math.Atan2(a.z - center.z, a.x - center.x);
        double theta2 = Math.Atan2(b.z - center.z, b.x - center.x);
        //Console.WriteLine(theta1);
        //Console.WriteLine(theta2);
        //Console.WriteLine();
        return (int)Math.Round(theta1 - theta2);

    }));


    Triangle[] triangles = new Triangle[polygon.Length - 2];
    int tri = 0;
    for (int i = 1; i < polygon.Length - 1; i++)
    {
        triangles[tri++] = new Triangle(new Vector2(polygon[0].x, polygon[0].z), new Vector2(polygon[i].x, polygon[i].z), new Vector2(polygon[i + 1].x, polygon[i + 1].z),
                                        polygon[0].y, polygon[i].y, polygon[i + 1].y,
                                        originalTriangle.UVAt(new Vector2(polygon[0].x, polygon[0].z)),
                                        originalTriangle.UVAt(new Vector2(polygon[i].x, polygon[i].z)),
                                        originalTriangle.UVAt(new Vector2(polygon[i + 1].x, polygon[i + 1].z)),
                                        originalTriangle.NormalAt(new Vector2(polygon[0].x, polygon[0].z)),
                                        originalTriangle.NormalAt(new Vector2(polygon[i].x, polygon[i].z)),
                                        originalTriangle.NormalAt(new Vector2(polygon[i + 1].x, polygon[i + 1].z)));
        //Console.WriteLine(triangles[tri - 1].vn0);
        //Console.WriteLine(triangles[tri - 1].vn1);
        //Console.WriteLine(triangles[tri - 1].vn2);
        //Console.WriteLine();
    }
    return triangles;
}*/
/*
public void DrawTriangle(Triangle tri, DirectBitmap b, float[] depthBuffer, Vector3 normal, Action<Fragment> frag)
{

    Fragment fragment = new Fragment();
    float zBuf;
    int padding = 5;
    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
    foreach (Vector2 v in tri.Points) //Get bounding box
    {
        if (v.x > maxX)
            maxX = (int)(v.x + 1);//Round up
        if (v.x < minX)
            minX = (int)(v.x);
        if (v.y > maxY)
            maxY = (int)(v.y + 1);
        if (v.y < minY)
            minY = (int)(v.y);
    }
    Vector2 pt = new Vector2();
    maxX = (maxX + padding > renderWidth - 1 ? renderWidth - 1 : maxX + padding);
    maxY = (maxY + padding > renderHeight - 1 ? renderHeight - 1 : maxY + padding);
    minX = (minX - padding < 0 ? 0 : minX - padding);
    minY = (minY - padding < 0 ? 0 : minY - padding);
    float z;
    for (int i = minX; i < maxX; i++)
    {
        for (int j = minY + 1; j <= maxY; j++)
        {
            pt.x = i + 0.5f;
            pt.y = j + 0.5f;
            Vector3 baryCoords = tri.GetBarycentricCoordinates(pt, out bool inside);
            if (inside)
            {
                //Do some interpolation with the z-buffer here
                z = tri.ZAt(baryCoords);
                zBuf = depthBuffer[(i) + (renderHeight * (renderHeight - j))];

                if (zBuf == 0 || z < zBuf)
                {

                    fragment.uv = tri.UVAt(baryCoords);
                    fragment.normal = tri.NormalAt(baryCoords);
                    fragment.color = Color.HotPink;
                    fragment.triangle = tri;
                    fragment.faceNormal = normal;
                    fragment.camera = this;
                    fragment.x = i;
                    fragment.y = j;
                    fragment.z = z;
                    frag(fragment);


                    b.SetPixel(fragment.x, renderHeight - fragment.y, fragment.color);
                    depthBuffer[(fragment.x) + (renderHeight * (renderHeight - fragment.y))] = fragment.z;
                }
            }
        }
    }
}*/