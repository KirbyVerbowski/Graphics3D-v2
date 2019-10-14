using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RequireComponentAttribute : Attribute
    {
        public Type component;
        public RequireComponentAttribute(Type component)
        {
            this.component = component;
        }
    }

    public abstract class Component
    {
        public virtual void Start() { }
        public virtual void Update() { }

        public Object3D object3D;
    }

    public class MeshComponent : Component
    {
        public Mesh mesh;
        public Mesh TransformedMesh {
            get; private set;
        }

        public override void Start()
        {
            TransformedMesh = new Mesh(mesh);
            object3D.transform.transformUpdate = UpdateMesh;
            UpdateMesh(TransformOps.All);
        }

        private void UpdateMesh(TransformOps ops)
        {
            for (int i = 0; i < TransformedMesh.vertices.Length; i++)
            {
                TransformedMesh.vertices[i] = object3D.transform.Rotation.RotateVector3(mesh.vertices[i] * object3D.transform.Scale) + object3D.transform.Location;
                if (i < mesh.vertexNormalCoords.Length)
                    TransformedMesh.vertexNormalCoords[i] = object3D.transform.Rotation.RotateVector3(mesh.vertexNormalCoords[i] * object3D.transform.Scale) + object3D.transform.Location;
            }

            if ((ops & TransformOps.Rotation) != 0)
            {
                TransformedMesh.CalculateFaceNormals();
            }
        }
    }

    [RequireComponent(typeof(MeshComponent))]
    public class MeshRenderer : Renderer
    {
        public Color albedo;
        public FragmentShaderDelegate fShader = null;
        public VertexShaderDelegate vShader = null;
        public List<object> shaderParams = new List<object>();


        private Camera camera;
        private Mesh mesh;
        private DirectBitmap b;

        public void FlatShader(Fragment f)
        {
            float fac = Vector3.Dot(f.faceNormal, -f.camera.transform.Forward);
            f.color = Color.FromArgb(albedo.A, (byte)(fac * albedo.R), (byte)(fac * albedo.G), (byte)(fac * albedo.B));
        }

        public override void Render(DirectBitmap b, Camera camera)
        {
            this.camera = camera;
            mesh = ((MeshComponent)object3D.GetComponent(typeof(MeshComponent))).TransformedMesh;
            this.b = b;
            Render();
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
            if ((e1.y > camera.farClip && e2.y > camera.farClip) ||
                 e1.y < camera.nearClip && e2.y < camera.nearClip) //Both either too close or too far
            {
                return false;
            }
            float slopeXY = (e1.x - e2.x) / (e1.y - e2.y);
            float slopeZY = (e1.z - e2.z) / (e1.y - e2.y);

            if ((e1.y > camera.farClip && e2.y < camera.farClip) || (e1.y < camera.farClip && e2.y > camera.farClip))    //Line passes through y = farclip
            {
                if (e1.y > camera.farClip) //e1 exceeds far clipping plane
                {
                    e1.x = (camera.farClip - e2.y) / slopeXY + e2.x;
                    e1.y = camera.farClip;
                    e1.z = (camera.farClip - e2.y) / slopeZY + e2.z;
                    moved1 = true;
                }
                else
                {
                    e2.x = (camera.farClip - e1.y) / slopeXY + e1.x;
                    e2.y = camera.farClip;
                    e2.z = (camera.farClip - e1.y) / slopeZY + e1.z;
                    moved2 = true;
                }
            }

            if ((e1.y < camera.nearClip && e2.y > camera.nearClip) || (e1.y > camera.nearClip && e2.y < camera.nearClip))    //Line passes through y = nearclip
            {
                if (e1.y < camera.nearClip) //e1 exceeds near clipping plane
                {
                    e1.x = (camera.nearClip - e2.y) / slopeXY + e2.x;
                    e1.y = camera.nearClip;
                    e1.z = (camera.nearClip - e2.y) / slopeZY + e2.z;
                    moved1 = true;
                }
                else
                {
                    e2.x = (camera.nearClip - e1.y) / slopeXY + e1.x;
                    e2.y = camera.nearClip;
                    e2.z = (camera.nearClip - e1.y) / slopeZY + e1.z;
                    moved2 = true;
                }
            }
            return true;
        }

        private void DrawClipTriangle(Triangle tri, DirectBitmap b, float[] depthBuffer, Vector3 normal, FragmentShaderDelegate frag)
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
            if (maxX > camera.renderWidth)
                maxX = camera.renderWidth;
            if (minX < 0)
                minX = 0;
            if (maxY > camera.renderHeight)
                maxY = camera.renderHeight;
            if (minY < 0)
                minY = 0;
            Vector2 pt = new Vector2();
            maxX = (maxX + padding > camera.renderWidth - 1 ? camera.renderWidth - 1 : maxX + padding);
            maxY = (maxY + padding > camera.renderHeight - 1 ? camera.renderHeight - 1 : maxY + padding);
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
                        zBuf = depthBuffer[(i) + (camera.renderHeight * (camera.renderHeight - j))];

                        if (zBuf == 0 || z < zBuf)
                        {

                            fragment.uv = tri.UVAt(baryCoords);
                            fragment.normal = tri.NormalAt(baryCoords);
                            fragment.color = Color.HotPink;
                            fragment.triangle = tri;
                            fragment.faceNormal = normal;
                            fragment.camera = camera;
                            fragment.thisMesh = mesh;
                            fragment.x = i;
                            fragment.y = j;
                            fragment.z = z;
                            frag(fragment);


                            b.SetPixel(fragment.x, camera.renderHeight - fragment.y, fragment.color);
                            depthBuffer[(fragment.x) + (camera.renderHeight * (camera.renderHeight - fragment.y))] = fragment.z;
                        }
                    }
                }
            }
        }

        private void Render()
        {
            Array.Clear(camera.depthBuffer, 0, camera.depthBuffer.Length);

            //Console.WriteLine(transform.Rotation.RotateVector3(obj.transform.Location - camPos));
            fShader = (fShader ?? FlatShader);
            vShader = (vShader ?? new VertexShaderDelegate((x)=> { }));
            for (int face = 0; face < mesh.faces.GetLength(0); face++)
            {
                float dot;
                dot = Vector3.Dot(mesh.faceNormals[face], -camera.transform.Forward);
                if (dot < 0) //Back face culling
                    continue;

                //Region vertex shader
                camera.vertex.location = mesh.vertices[mesh.faces[face, 0]] + object3D.transform.Location;
                camera.vertex.objectPos = object3D.transform.Location;
                camera.vertex.camera = camera;

                vShader(camera.vertex);
                camera.vert1 = camera.transform.Rotation.Conjugate.RotateVector3(camera.vertex.location - camera.transform.Location);

                camera.vertex.location = mesh.vertices[mesh.faces[face, 1]] + object3D.transform.Location;
                camera.vertex.objectPos = object3D.transform.Location;
                camera.vertex.camera = camera;

                vShader(camera.vertex);
                camera.vert2 = camera.transform.Rotation.Conjugate.RotateVector3(camera.vertex.location - camera.transform.Location);

                camera.vertex.location = mesh.vertices[mesh.faces[face, 2]] + object3D.transform.Location;
                camera.vertex.objectPos = object3D.transform.Location;
                camera.vertex.camera = camera;

                vShader(camera.vertex);
                camera.vert3 = camera.transform.Rotation.Conjugate.RotateVector3(camera.vertex.location - camera.transform.Location);
                //End region vertex shader


                camera.e1.x = (camera.projectionDistance * camera.vert1.x) / camera.vert1.y;           //Using similar triangles to project the verts onto the camera plane 
                camera.e1.z = (camera.projectionDistance * camera.vert1.z) / camera.vert1.y;
                camera.e2.x = (camera.projectionDistance * camera.vert2.x) / camera.vert2.y;
                camera.e2.z = (camera.projectionDistance * camera.vert2.z) / camera.vert2.y;
                camera.e3.x = (camera.projectionDistance * camera.vert3.x) / camera.vert3.y;
                camera.e3.z = (camera.projectionDistance * camera.vert3.z) / camera.vert3.y;

                camera.e1.y = camera.vert1.y;
                camera.e2.y = camera.vert2.y;                 //Don't change e.y because we will use it for z-buffering later
                camera.e3.y = camera.vert3.y;

                camera.e1.x *= camera.screenNormCoeffX;
                camera.e1.z *= camera.screenNormCoeffZ;   //Bound x and z to the range [-1,1]
                camera.e2.x *= camera.screenNormCoeffX;
                camera.e2.z *= camera.screenNormCoeffZ;
                camera.e3.x *= camera.screenNormCoeffX;
                camera.e3.z *= camera.screenNormCoeffZ;


                //camera.projectedTriangle.v0 = new Vector2(camera.e1.x, camera.e1.z);
                //camera.projectedTriangle.v1 = new Vector2(camera.e2.x, camera.e2.z);
                //camera.projectedTriangle.v2 = new Vector2(camera.e3.x, camera.e3.z);
                camera.projectedTriangle.z0 = camera.e1.y;
                camera.projectedTriangle.z1 = camera.e2.y;
                camera.projectedTriangle.z2 = camera.e3.y;
                camera.projectedTriangle.uv0 = mesh.uvcoords[mesh.uvs[face, 0]];
                camera.projectedTriangle.uv1 = mesh.uvcoords[mesh.uvs[face, 1]];
                camera.projectedTriangle.uv2 = mesh.uvcoords[mesh.uvs[face, 2]];
                camera.projectedTriangle.vn0 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 0]];
                camera.projectedTriangle.vn1 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 1]];
                camera.projectedTriangle.vn2 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 2]];


                //New stuff: still need a solution to near/far clip
                camera.projectedTriangle.v0 = new Vector2(camera.renderWidth / 2f + (camera.e1.x * camera.normToScreen.x), camera.renderHeight / 2f + (camera.e1.z * camera.normToScreen.z));
                camera.projectedTriangle.v1 = new Vector2(camera.renderWidth / 2f + (camera.e2.x * camera.normToScreen.x), camera.renderHeight / 2f + (camera.e2.z * camera.normToScreen.z));
                camera.projectedTriangle.v2 = new Vector2(camera.renderWidth / 2f + (camera.e3.x * camera.normToScreen.x), camera.renderHeight / 2f + (camera.e3.z * camera.normToScreen.z));

                DrawClipTriangle(camera.projectedTriangle, b, camera.depthBuffer, mesh.faceNormals[face], fShader);

            }
        }
    }

    public abstract class Renderer : Component
    {
        public abstract void Render(DirectBitmap b, Camera camera);
    }

    public class CameraComponent : Component
    {
        private DirectBitmap b;
        private object canvasLock;
        private Camera camera;

        public override void Start()
        {
            camera = (Camera)object3D;
            object3D.scene.canvas = new DirectBitmap(camera.renderWidth, camera.renderHeight);
            b = object3D.scene.canvas;
            canvasLock = object3D.scene.canvasLock;
        }
        public override void Update()
        {
            lock (canvasLock)
            {
                b.LockBits();
                b.ClearColor(object3D.scene.worldColor.ToArgb());
                foreach (Object3D obj in object3D.scene.sceneObjects)
                {
                    if (obj.visible && obj.components.Exists((x)=>(x.BaseType == typeof(Renderer))))
                    {
                        Renderer r = (Renderer)object3D.scene.componentInstances[obj].Find((x) => (x.GetType().BaseType == typeof(Renderer)));
                        r.Render(b, camera);
                    }
                }
                b.UnlockBits();
            }
            object3D.scene.PaintEvent(b);
        }
    }
}
