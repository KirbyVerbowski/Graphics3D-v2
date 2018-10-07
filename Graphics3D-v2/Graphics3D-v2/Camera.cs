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
        private RenderMode renderMode = RenderMode.Wireframe;
        public bool orthographic = true;
        float projectionDistance = 1;
        float horizFOV = 0.8552f; //approx. 49 degrees
        float vertFOV;
        float aspectRatio;
        float nearClip = 0.1f;
        float farClip = 100;

        public Camera(Transform transform, int width, int height, float horizFOV) : base(transform, new Mesh(@"..\..\Resources\Camera.obj"))
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
                        if (NearFarClip(ref e1, ref e2)) //Gonna have to do something new here to clip the triangle 
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

            return true;
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
