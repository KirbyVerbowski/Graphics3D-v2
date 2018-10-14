using System;
using System.Collections.Generic;
using System.Drawing;


namespace Graphics3D_v2
{
    class Camera : Object3D
    {

        private class Triangle
        {
            public Vector3 v0, v1, v2;
            public Vector3[] Points {
                get {
                    return new Vector3[] { v0, v1, v2 };
                }
            }
            public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                this.v0 = v0; this.v1 = v1; this.v2 = v2;
            }

            
        }

        public class Triangle2
        {
            public Vector2 v0, v1, v2;
            public float z0, z1, z2;
            public Vector2[] Points {
                get { return new Vector2[] { v0, v1, v2 }; }
            }
            public Triangle2(Vector2 v0, Vector2 v1, Vector2 v2, float z0, float z1, float z2)
            {
                this.v0 = v0; this.v1 = v1; this.v2 = v2;
                this.z0 = z0; this.z1 = z1; this.z2 = z2;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private float EdgeFunction(Vector2 a, Vector2 b, Vector2 c, out bool result)
            {
                float fresult = -((c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x));
                result = fresult >= 0;
                return fresult;
            }
            private float EdgeFunction(Vector2 a, Vector2 b, Vector2 c)
            {
                return -((c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x));
            }

            public bool InsideTriangle(Vector2 point)
            {
                EdgeFunction(v0, v1, point, out bool result1);
                EdgeFunction(v1, v2, point, out bool result2);
                EdgeFunction(v2, v0, point, out bool result3);
                return result1 && result2 && result3;
            }

            public Vector3 GetBarycentricCoordinates(Vector2 point)
            {
                float area = EdgeFunction(v0, v1, v2);
                float w0 = EdgeFunction(v1, v2, point); 
                float w1 = EdgeFunction(v2, v0, point); 
                float w2 = EdgeFunction(v0, v1, point);
                return new Vector3(w0 / area, w1 / area, w2 / area);
            }
            public Vector3 GetBarycentricCoordinates(Vector2 point, out bool inTri)
            {
                float area = EdgeFunction(v0, v1, v2);
                float w0 = EdgeFunction(v1, v2, point, out bool res1);
                float w1 = EdgeFunction(v2, v0, point, out bool res2);
                float w2 = EdgeFunction(v0, v1, point, out bool res3);
                inTri = res1 && res2 && res3;
                return new Vector3(w0 / area, w1 / area, w2 / area);
            }

            public float ZAt(Vector3 baryCentricCoords)
            {
                return z0 * baryCentricCoords.x + z1 * baryCentricCoords.y + z2 * baryCentricCoords.z;
            }

            public override string ToString()
            {
                return "(" + v0 + v1 + v2 + ")";
            }
        }

        enum RenderMode { Wireframe, Solid }

        public List<Object3D> renderQueue = new List<Object3D>();
        public int renderWidth = 512;
        public int renderHeight = 256;
        private RenderMode renderMode = RenderMode.Solid;
        public bool orthographic = true;
        float projectionDistance = 1;
        float horizFOV = 0.8552f; //approx. 49 degrees
        float vertFOV;
        float aspectRatio;
        float nearClip = 1f;
        float farClip = 100;

        public Camera(Transform transform, int width, int height, float horizFOV) : base(transform, new Mesh(@"..\..\Camera.obj"))
        {
            renderHeight = height;
            renderWidth = width;
            aspectRatio = (float)width / (float)height;
            this.horizFOV = horizFOV;
            vertFOV = horizFOV / aspectRatio;
        }


        //False if both off screen
        private bool EdgeClip(ref Vector3 e1, ref Vector3 e2, out bool moved1, out bool moved2)
        {
            Vector3 bef1 = new Vector3(e1.x, e1.y, e1.z), bef2 = new Vector3(e2.x, e2.y, e2.z);
            moved1 = false; moved2 = false;
            if ((e1.x < -1 && e2.x < -1) ||
                            (e1.x > 1 && e2.x > 1)) //Both Points off the screen to left or right
            {
                return false;
            }

            float slope = (e1.z - e2.z) / (e1.x - e2.x);

            if ((e1.x > 1 && e2.x < 1) || (e1.x < 1 && e2.x > 1))  //Edge pases through x = 1
            {
                if (e1.x < 1) //e1 on the screen
                {
                    e2.x = 1;
                    e2.z = slope * (1 - e1.x) + e1.z;
                    moved2 = true;
                }
                else
                {
                    e1.x = 1;
                    e1.z = slope * (1 - e2.x) + e2.z;
                    moved1 = true;
                }
            }
            if ((e1.x < -1 && e2.x > -1) || (e1.x > -1 && e2.x < -1))  //Edge pases through x = -1
            {
                if (e1.x > -1) //e1 on the screen
                {
                    e2.x = -1;
                    e2.z = slope * (-1 - e1.x) + e1.z;
                    moved2 = true;
                }
                else
                {
                    e1.x = -1;
                    e1.z = slope * (-1 - e2.x) + e2.z;
                    moved1 = true;
                }
            }

            if ((e1.z < -1 && e2.z < -1) ||
                (e1.z > 1 && e2.z > 1)) //Both Points off the screen above or below
            {
                return false;
            }

            if ((e1.z > 1 && e2.z < 1) || (e1.z < 1 && e2.z > 1)) //Edge passes through z = 1
            {
                if (e1.z < 1) //e1 on the screen
                {
                    e2.z = 1;
                    e2.x = (1 - e1.z) / slope + e1.x;
                    moved2 = true;
                }
                else
                {
                    e1.z = 1;
                    e1.x = (1 - e2.z) / slope + e2.x;
                    moved1 = true;
                }
            }
            if ((e1.z > -1 && e2.z < -1) || (e1.z < -1 && e2.z > -1))  //Edge passes through z = -1
            {
                if (e1.z > -1) //e1 on the screen
                {
                    e2.z = -1;
                    e2.x = (-1 - e1.z) / slope + e1.x;
                    moved2 = true;
                }
                else
                {
                    e1.z = -1;
                    e1.x = (-1 - e2.z) / slope + e2.x;
                    moved1 = true;
                }
            }
            return true;
        }

        //False if both off screen
        private bool NearFarClip(ref Vector3 e1, ref Vector3 e2, out bool moved1, out bool moved2)
        {
            moved1 = false; moved2 = false;
            if((e1.y > farClip && e2.y > farClip) ||
                 e1.y < nearClip && e2.y < nearClip) //Both either too close or too far
            {
                return false;
            }
            float slopeXY = (e1.x - e2.x) / (e1.y - e2.y);
            float slopeZY = (e1.z - e2.z) / (e1.y - e2.y);

            if((e1.y > farClip && e2.y < farClip) || (e1.y < farClip && e2.y > farClip))    //Line passes through y = farclip
            {
                if (e1.y > farClip) //e1 exceeds far clipping plane
                {
                    e1.x = (farClip - e2.y) / slopeXY + e2.x;
                    e1.y = farClip;
                    e1.z = (farClip - e2.y) / slopeZY + e2.z;
                    moved1 = true;
                }
                else
                {
                    e2.x = (farClip - e1.y) / slopeXY + e1.x;
                    e2.y = farClip;
                    e2.z = (farClip - e1.y) / slopeZY + e1.z;
                    moved2 = true;
                }
            }

            if((e1.y < nearClip && e2.y > nearClip) || (e1.y > nearClip && e2.y < nearClip))    //Line passes through y = nearclip
            {
                if(e1.y < nearClip) //e1 exceeds near clipping plane
                {
                    e1.x = (nearClip - e2.y) / slopeXY + e2.x;
                    e1.y = nearClip;
                    e1.z = (nearClip - e2.y) / slopeZY + e2.z;
                    moved1 = true;
                }
                else
                {
                    e2.x = (nearClip - e1.y) / slopeXY + e1.x;
                    e2.y = nearClip;
                    e2.z = (nearClip - e1.y) / slopeZY + e1.z;
                    moved2 = true;
                }
            }
            return true;
        }

        public bool Render(DirectBitmap b)
        {
            System.Diagnostics.Debug.Assert(b.Width == renderWidth && b.Height == renderHeight);

            Mesh mesh;
            Vector3 camPos = transform.Location, vert1, vert2, vert3;
            Vector3 e1, e2, e3; //Normalized between -1 and 1
            Vector3 beforeClip1, beforeClip2, beforeClip3;
            Vector3 coordTransform = new Vector3(renderWidth / 2, 0, renderHeight / 2);
            Vector3 normToScreen = new Vector3(renderWidth / 2, 1, renderHeight / 2);
            float screenNormCoeffX = 1 / (projectionDistance * (float)Math.Tan(horizFOV));
            float screenNormCoeffZ = 1 / (projectionDistance * (float)Math.Tan(vertFOV));
            float[] depthBuffer = new float[renderWidth * renderHeight];

            if(renderMode == RenderMode.Wireframe)
            {
                foreach (Object3D obj in renderQueue)
                {
                    mesh = obj.TransformedMesh;
                    for(int i = 0; i < mesh.edges.GetLength(0); i++)
                    {
                        //Put vertecies in coordinates relative to the camera
                        vert1 = transform.Rotation.Conjugate.RotateVector3(mesh.vertices[mesh.edges[i, 0]] - camPos);
                        vert2 = transform.Rotation.Conjugate.RotateVector3(mesh.vertices[mesh.edges[i, 1]] - camPos);

                        e1.x = (projectionDistance * vert1.x) / vert1.y;           //Using similar triangles to project the verts onto the camera plane 
                        e1.z = (projectionDistance * vert1.z) / vert1.y;
                        e2.x = (projectionDistance * vert2.x) / vert2.y;
                        e2.z = (projectionDistance * vert2.z) / vert2.y;
                        e1.y = vert1.y;
                        e2.y = vert2.y;                 //Don't change e.y because we will use it for z-buffering later

                        e1.x *= screenNormCoeffX;
                        e1.z *= screenNormCoeffZ;   //Bound x and z to the range [-1,1]
                        e2.x *= screenNormCoeffX;
                        e2.z *= screenNormCoeffZ;

                        //Do Edge clipping
                        /* Shit's fucked atm and idgaf about wireframe
                        if (NearFarClip(ref e1, ref e2))
                        {
                            if (EdgeClip(ref e1, ref e2))
                            {
                                e1 *= normToScreen; // x in [-width/2, width/2] z in [-height/2, height/2]
                                e2 *= normToScreen;

                                e1 = new Vector3(renderWidth / 2f + e1.x, e1.y, renderHeight - (renderHeight / 2f + e1.z)); //x in [0,width] z in [0,height]
                                e2 = new Vector3(renderWidth / 2f + e2.x, e2.y, renderHeight - (renderHeight / 2f + e2.z));
                                foreach (Tuple<int, int> pt in GetLinePixels(e1, e2))
                                {
                                    b.SetPixel(pt.Item1, pt.Item2, Color.Black);

                                }

                            }
                        }
                        */
                    }
                }
                
            }
            else  //Solid shading
            {
                foreach (Object3D obj in renderQueue)
                {
                    mesh = obj.TransformedMesh;
                    for(int face = 0; face < mesh.faces.GetLength(0); face++)
                    {
                        float dot = Vector3.Dot(mesh.faceNormals[face], -transform.Forward);
                        if (dot < 0) //Back face culling
                            continue;
                        Color faceColor = Color.FromArgb((int)(255 * dot), 0, 0);

                        vert1 = transform.Rotation.Conjugate.RotateVector3(mesh.vertices[mesh.faces[face, 0]] - camPos);
                        vert2 = transform.Rotation.Conjugate.RotateVector3(mesh.vertices[mesh.faces[face, 1]] - camPos);
                        vert3 = transform.Rotation.Conjugate.RotateVector3(mesh.vertices[mesh.faces[face, 2]] - camPos);

                        e1.x = (projectionDistance * vert1.x) / vert1.y;           //Using similar triangles to project the verts onto the camera plane 
                        e1.z = (projectionDistance * vert1.z) / vert1.y;
                        e2.x = (projectionDistance * vert2.x) / vert2.y;
                        e2.z = (projectionDistance * vert2.z) / vert2.y;
                        e3.x = (projectionDistance * vert3.x) / vert3.y;
                        e3.z = (projectionDistance * vert3.z) / vert3.y;

                        e1.y = vert1.y;
                        e2.y = vert2.y;                 //Don't change e.y because we will use it for z-buffering later
                        e3.y = vert3.y;

                        e1.x *= screenNormCoeffX;
                        e1.z *= screenNormCoeffZ;   //Bound x and z to the range [-1,1]
                        e2.x *= screenNormCoeffX;
                        e2.z *= screenNormCoeffZ;
                        e3.x *= screenNormCoeffX;
                        e3.z *= screenNormCoeffZ;

                        beforeClip1 = e1;
                        beforeClip2 = e2;
                        beforeClip3 = e3;
                        List<Vector3> drawPoints = new List<Vector3>(); //Will be a poygon with up to 6 sides
                        bool added1 = false, added2 = false, added3 = false;

                        if(NearFarClip(ref e1, ref e2, out bool moved1nf, out bool moved2nf))
                        {
                            if(EdgeClip(ref e1, ref e2, out bool moved1ec, out bool moved2ec))
                            {
                                if(moved1ec || moved1nf)
                                {
                                    drawPoints.Add(e1);
                                }
                                else if(!added1)
                                {
                                    drawPoints.Add(e1); added1 = true;
                                }
                                if (moved2ec || moved2nf)
                                {
                                    drawPoints.Add(e1);
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

                        for (int i = 0; i < drawPoints.Count; i++)
                        {
                            drawPoints[i] = new Vector3(renderWidth / 2f + (drawPoints[i].x * normToScreen.x), drawPoints[i].y, (renderHeight / 2f + (drawPoints[i].z * normToScreen.z)));
                        }
                        if(drawPoints.Count > 3)
                        {
                            Triangle2[] tris = Triangulate(drawPoints.ToArray());
                            foreach (Triangle2 tri in tris)
                            {
                                DrawTriangle(tri, b, depthBuffer, faceColor);
                            }
                        }
                        else if(drawPoints.Count == 3)
                        {
                            Triangle2 tri = new Triangle2(drawPoints[0], drawPoints[1], drawPoints[2], e1.y, e2.y, e3.y);
                            DrawTriangle(tri, b, depthBuffer, faceColor);
                        }
                       
                    }
                }
            }
            return true;
        }
       

        //Takes a convex, counterclockwise winding n-gon and returns n - 2 list of triangles (fan method)
        private Triangle2[] Triangulate(Vector3[] polygon)
        {
            Triangle2[] triangles = new Triangle2[polygon.Length - 2];
            int tri = 0;
            for(int i = 1; i < polygon.Length - 1; i++)
            {
                triangles[tri++] = new Triangle2(polygon[0], polygon[i], polygon[i + 1], polygon[0].y, polygon[i].y, polygon[i+1].y);
            }
            return triangles;
        }

        Tuple<int, int>[] GetLinePixels(Vector3 from, Vector3 to)
        {
            List<Tuple<int, int>> points = new List<Tuple<int, int>>();
            float slope = (to.z - from.z) / (to.x - from.x);
            int sx, sz, ex, ez;
            if(from.x < to.x)
            {
                sx = (int)Math.Round(from.x); sz = (int)Math.Round(from.z);
                ex = (int)Math.Round(to.x); ez = (int)Math.Round(to.z);
            }
            else
            {
                ex = (int)Math.Round(from.x); ez = (int)Math.Round(from.z);
                sx = (int)Math.Round(to.x); sz = (int)Math.Round(to.z);
            }
            if(sx == ex)
            {
                return points.ToArray();
            }
            
            points.Add(new Tuple<int, int>(sx, sz));
            for(int x = sx+1; x < ex; x++)
            {
                points.Add(new Tuple<int, int>(x, (int)Math.Round(slope * (x - sx) + sz)));
            }
            return points.ToArray();
        }

        public void DrawTriangle(Triangle2 tri, DirectBitmap b, float[] depthBuffer, Color color)
        {
            float zBuf;
            int padding = 5;
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach(Vector2 v in tri.Points) //Get bounding box
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
            maxX = (maxX + padding > renderWidth - 1 ? renderWidth-1 : maxX + padding);
            maxY = (maxY + padding > renderHeight - 1 ? renderHeight-1 : maxY + padding);
            minX = (minX - padding < 0 ? 0 : minX - padding);
            minY = (minY - padding < 0 ? 0 : minY - padding);
            float z;
            for(int i = minX; i < maxX; i++)
            {
                for(int j = minY + 1; j <= maxY; j++)
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
                            b.SetPixel(i, renderHeight - j, color);
                            depthBuffer[(i) + (renderHeight * (renderHeight-j))] = z;
                        }
                        else
                        {
                            //Console.WriteLine(z + " buff at " + zBuf);
                        }
                    }
                }
            }
        }
    
    }
}
