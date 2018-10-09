using System;
using System.Collections.Generic;
using System.Drawing;


namespace Graphics3D_v2
{
    class Camera : Object3D
    {

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
        float nearClip = 0.1f;
        float farClip = 100;

        public Camera(Transform transform, int width, int height, float horizFOV) : base(transform, new Mesh(@"..\..\Camera.obj"))
        {
            renderHeight = height;
            renderWidth = width;
            aspectRatio = (float)width / (float)height;
            this.horizFOV = horizFOV;
            vertFOV = horizFOV / aspectRatio;
        }


        //True return value means the vectors were changed
        private bool edgeClip(ref Vector3 e1, ref Vector3 e2)
        {
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
                }
                else
                {
                    e1.x = 1;
                    e1.z = slope * (1 - e2.x) + e2.z;
                }
            }
            if ((e1.x < -1 && e2.x > -1) || (e1.x > -1 && e2.x < -1))  //Edge pases through x = -1
            {
                if (e1.x > -1) //e1 on the screen
                {
                    e2.x = -1;
                    e2.z = slope * (-1 - e1.x) + e1.z;
                }
                else
                {
                    e1.x = -1;
                    e1.z = slope * (-1 - e2.x) + e2.z;
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
                }
                else
                {
                    e1.z = 1;
                    e1.x = (1 - e2.z) / slope + e2.x;
                }
            }
            if ((e1.z > -1 && e2.z < -1) || (e1.z < -1 && e2.z > -1))  //Edge passes through z = -1
            {
                if (e1.z > -1) //e1 on the screen
                {
                    e2.z = -1;
                    e2.x = (-1 - e1.z) / slope + e1.x;
                }
                else
                {
                    e1.z = -1;
                    e1.x = (-1 - e2.z) / slope + e2.x;
                }
            }

            return true;
        }

        //True return indicates one of the vectors was moved
        private bool NearFarClip(ref Vector3 e1, ref Vector3 e2)
        {
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
                }
                else
                {
                    e2.x = (farClip - e1.y) / slopeXY + e1.x;
                    e2.y = farClip;
                    e2.z = (farClip - e1.y) / slopeZY + e1.z;
                }
            }

            if((e1.y < nearClip && e2.y > nearClip) || (e1.y > nearClip && e2.y < nearClip))    //Line passes through y = nearclip
            {
                if(e1.y < nearClip) //e1 exceeds near clipping plane
                {
                    e1.x = (nearClip - e2.y) / slopeXY + e2.x;
                    e1.y = nearClip;
                    e1.z = (nearClip - e2.y) / slopeZY + e2.z;
                }
                else
                {
                    e2.x = (nearClip - e1.y) / slopeXY + e1.x;
                    e2.y = nearClip;
                    e2.z = (nearClip - e1.y) / slopeZY + e1.z;
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
                        if (NearFarClip(ref e1, ref e2))
                        {
                            if (edgeClip(ref e1, ref e2))
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
                        if(dot < 0) //Back face culling
                        {
                            continue;
                        }
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
                        if(NearFarClip(ref e1, ref e2))
                        {
                            if(edgeClip(ref e1, ref e2))
                            {
                                drawPoints.Add(e1);
                                drawPoints.Add(e2);
                            }
                        }
                        e1 = beforeClip1;
                        e2 = beforeClip2;
                        if (NearFarClip(ref e2, ref e3))
                        {
                            if (edgeClip(ref e2, ref e3))
                            {
                                drawPoints.Add(e2);
                                drawPoints.Add(e3);
                            }
                        }
                        e2 = beforeClip2;
                        e3 = beforeClip3;
                        if (NearFarClip(ref e1, ref e3))
                        {
                            if (edgeClip(ref e1, ref e3))
                            {
                                drawPoints.Add(e3);
                                drawPoints.Add(e1);
                            }
                        }

                        for(int i = 0; i < drawPoints.Count; i++)
                        {
                            drawPoints[i] = new Vector3(renderWidth / 2f + (drawPoints[i].x * normToScreen.x), drawPoints[i].y, renderHeight - (renderHeight / 2f + (drawPoints[i].z * normToScreen.z)));
                        }

                        FillPolygon(drawPoints.ToArray(), b, depthBuffer, faceColor);
                        

                       
                    }
                }
            }

            return true;
        }
        

        //----> +x
    //   |
    //   \/
    //   +z
        void FillPolygon(Vector3[] pts, DirectBitmap b, float[] depthBuffer, Color color)
        {
            List<Tuple<int, float, int>> points = new List<Tuple<int, float, int>>();
            int maxZ = int.MinValue;
            int minZ = int.MaxValue;
            int rounded, rounded2, max;
            Tuple<int, float, int> temp;
            int x1, z1, x2, z2;
            float slopeXZ, slopeYZ;
            foreach (Vector3 pt in pts)
            {
                if ((rounded = (int)Math.Round(pt.z)) > maxZ)
                    maxZ = rounded;
                if (rounded < minZ)
                    minZ = rounded;
            }
            int?[] xMinMax = new int?[maxZ - minZ]; 
            for(int i = 0; i < pts.Length - 1; i++) //For each edge (except for the one that connects the last to the first vertex
            {
                slopeXZ = (pts[i + 1].z - pts[i].z) / (pts[i+1].x - pts[i].x);
                slopeYZ = (pts[i + 1].z - pts[i].z) / (pts[i + 1].y - pts[i].y);
                x1 = (int)Math.Round(pts[i].x); z1 = (int)Math.Round(pts[i].z);
                x2 = (int)Math.Round(pts[i + 1].x); z2 = (int)Math.Round(pts[i + 1].z);

                if (x1 == x2) //Vertical line
                {
                    max = Math.Max(z1, z2);
                    for(int j = Math.Min(z1, z2); j < max; j++)
                    {
                        if (xMinMax[j - minZ] == null)
                        {
                            xMinMax[j - minZ] = x1;
                        }
                        else
                        {
                            if(xMinMax[j - minZ] > x1)
                            {
                                for(int k = x1; k < xMinMax[j - minZ]; k++) //Color the row of pixels
                                {
                                    //if (depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] == 0 || depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] > temp.Item2)
                                    //{
                                        b.SetPixel(k, j, color);
                                    //}
                                }
                            }else
                            {
                                for(int k = (int)xMinMax[j-minZ]; k < x1; k++)
                                {
                                    b.SetPixel(k, j, color);    //do z-buffering here
                                }
                            }
                        }
                    }
                }
                else if(z1 != z2)   //Diagonal line
                {
                    max = Math.Max(z1, z2);
                    for (int j = Math.Min(z1, z2); j < max; j++)
                    {
                        temp = new Tuple<int, float, int>((int)Math.Round(((j - pts[i].z) / slopeXZ) + pts[i].x), ((j - pts[i].z) / slopeYZ) + pts[i].y, j);
                        if (xMinMax[j - minZ] == null)
                        {
                            xMinMax[j - minZ] = temp.Item1;
                        }
                        else
                        {
                            if (xMinMax[j - minZ] > temp.Item1)
                            {
                                for (int k = temp.Item1; k < xMinMax[j - minZ]; k++) //Color the row of pixels
                                {
                                    //if (depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] == 0 || depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] > temp.Item2)
                                    //{
                                    b.SetPixel(k, j, color);
                                    //}
                                }
                            }
                            else
                            {
                                for (int k = (int)xMinMax[j - minZ]; k < temp.Item1; k++)
                                {
                                    b.SetPixel(k, j, color);    //Do z-buffering here
                                }
                            }
                        }
                    }
                }
                else //Horizontal line
                {
                    max = Math.Max(x1, x2);
                    for (int j = Math.Min(x1, x2); j < max; j++)
                    {
                        b.SetPixel(j, z1, color);   //Do z-buffering here
                    }
                }
            }
            slopeXZ = (pts[pts.Length-1].z - pts[0].z) / (pts[pts.Length-1].x - pts[0].x);
            slopeYZ = (pts[pts.Length-1].z - pts[0].z) / (pts[pts.Length-1].y - pts[0].y);
            x1 = (int)Math.Round(pts[pts.Length-1].x); z1 = (int)Math.Round(pts[pts.Length-1].z);
            x2 = (int)Math.Round(pts[0].x); z2 = (int)Math.Round(pts[0].z);

            if (x1 == x2) //Vertical line
            {
                max = Math.Max(z1, z2);
                for (int j = Math.Min(z1, z2); j < max; j++)
                {
                    if (xMinMax[j - minZ] == null)
                    {
                        xMinMax[j - minZ] = x1;
                    }
                    else
                    {
                        if (xMinMax[j - minZ] > x1)
                        {
                            for (int k = x1; k < xMinMax[j - minZ]; k++) //Color the row of pixels
                            {
                                //if (depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] == 0 || depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] > temp.Item2)
                                //{
                                b.SetPixel(k, j, color);
                                //}
                            }
                        }
                        else
                        {
                            for (int k = (int)xMinMax[j - minZ]; k < x1; k++)
                            {
                                b.SetPixel(k, j, color);    //do z-buffering here
                            }
                        }
                    }
                }
            }
            else if (z1 != z2)   //Diagonal line
            {
                max = Math.Max(z1, z2);
                for (int j = Math.Min(z1, z2); j < max; j++)
                {
                    temp = new Tuple<int, float, int>((int)Math.Round(((j - pts[pts.Length-1].z) / slopeXZ) + pts[pts.Length-1].x), ((j - pts[pts.Length-1].z) / slopeYZ) + pts[pts.Length-1].y, j);
                    if (xMinMax[j - minZ] == null)
                    {
                        xMinMax[j - minZ] = temp.Item1;
                    }
                    else
                    {
                        if (xMinMax[j - minZ] > temp.Item1)
                        {
                            for (int k = temp.Item1; k < xMinMax[j - minZ]; k++) //Color the row of pixels
                            {
                                //if (depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] == 0 || depthBuffer[temp.Item3 + (renderWidth * temp.Item1)] > temp.Item2)
                                //{
                                b.SetPixel(k, j, color);
                                //}
                            }
                        }
                        else
                        {
                            for (int k = (int)xMinMax[j - minZ]; k < temp.Item1; k++)
                            {
                                b.SetPixel(k, j, color);    //Do z-buffering here
                            }
                        }
                    }
                }
            }
            else //Horizontal line
            {
                max = Math.Max(x1, x2);
                for (int j = Math.Min(x1, x2); j < max; j++)
                {
                    b.SetPixel(j, z1, color);   //Do z-buffering here
                }
            }

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

        
    
    }
}
