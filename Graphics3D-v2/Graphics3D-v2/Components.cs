using System;
using System.Collections;
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

    public class Component
    {
        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
        public virtual DirectBitmap OnPreRender(DirectBitmap image) { return image; }
        public virtual DirectBitmap OnPostRender(DirectBitmap image) { return image; }

        public void StartCoroutine(IEnumerator method)
        {
            Task task = new Task(async () =>
            {
                YieldInstruction instruction = null;
                while (method.MoveNext())
                {
                    try
                    {
                        instruction = (YieldInstruction)method.Current ?? new WaitForUpdate();
                    }
                    catch (InvalidCastException)
                    {
                        Console.WriteLine("Coroutine did not yield a YieldInstruction, continuing as if it returned WaitForUpdate");
                        instruction = new WaitForUpdate();
                    }
                    finally
                    {
                        await instruction.beforeNext;
                    }
                }
            });
            task.Start();
        }

        public Object3D object3D;
    }

    public class MeshComponent : Component
    {
        public Mesh mesh;
        public Mesh TransformedMesh {
            get; private set;
        }
        public TransformUpdateDelegate updateMeshDelegate;

        public override void Awake()
        {
            if (updateMeshDelegate == null)
            {
                updateMeshDelegate = UpdateMesh;
            }
            TransformedMesh = new Mesh(mesh);
            object3D.transform.transformUpdate = updateMeshDelegate;
            updateMeshDelegate(TransformOps.All);
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

        public void SmoothShader(Fragment f)
        {
            float fac = Vector3.Dot(f.normal, -f.camera.transform.Forward);
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
            vShader = (vShader ?? new VertexShaderDelegate((x) => { }));
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

    [RequireComponent(typeof(MeshComponent))]
    public class RigComponent : Component
    {
        public Rig rig;
        MeshComponent mc;
        public override void Start()
        {
            mc = (MeshComponent)object3D.GetComponent(typeof(MeshComponent));
            mc.updateMeshDelegate = UpdateMesh;
        }

        private void UpdateMesh(TransformOps ops)
        {
            //Probably want to point mc.updatemeshdelegate at nothing and then have this 
            //be not an updatemeshdelegate and be called only when the pose changes
            //then fuck with the transformedmesh
        }
    }

    public abstract class Renderer : Component
    {
        public abstract void Render(DirectBitmap b, Camera camera);
    }

    public class CameraComponent : Component
    {

        private Camera camera;

        public override void Start()
        {
            camera = (Camera)object3D;
        }

        public override DirectBitmap OnPreRender(DirectBitmap image)
        {
            image.LockBits();
            image.ClearColor(GameManager.worldColor.ToArgb());
            foreach (Object3D obj in GameManager.sceneObjects)
            {
                if (obj.visible && obj.components.Exists((x) => (x.BaseType == typeof(Renderer))))
                {
                    Renderer r = (Renderer)GameManager.componentInstances[obj].Find((x) => (x.GetType().BaseType == typeof(Renderer)));
                    r.Render(image, camera);
                }
            }
            image.UnlockBits();
            return image;
        }

    }

    public abstract class Collider : Component
    {
        public abstract Mesh mesh { get; protected set; }
        public bool smoothMesh = false;

        public virtual bool RayCast(Ray ray, out RayCastHit hit)
        {
            Triangle3 hitTriangle = new Triangle3();
            Vector3 barycentricCoords;
            Vector3 pt;
            ray.direction.Normalize();
            for (int face = 0; face < mesh.faces.GetLength(0); face++)
            {
                //Ignore faces where ray hits from behind
                float rayNormalDot = Vector3.Dot(mesh.faceNormals[face], ray.direction);
                if (rayNormalDot >= 0)
                    continue;

                float planeConst = Vector3.Dot(mesh.faceNormals[face], mesh.vertices[mesh.faces[face, 0]] + object3D.transform.Location);
                float t = -(Vector3.Dot(mesh.faceNormals[face], ray.origin) + planeConst) / Vector3.Dot(mesh.faceNormals[face], ray.direction);
                //Triangle is behind ray origin
                if (t < 0)
                    continue;
                pt = ray.origin + (t * ray.direction);

                hitTriangle.v0 = mesh.vertices[mesh.faces[face, 0]] + object3D.transform.Location;
                hitTriangle.v1 = mesh.vertices[mesh.faces[face, 1]] + object3D.transform.Location;
                hitTriangle.v2 = mesh.vertices[mesh.faces[face, 2]] + object3D.transform.Location;
                hitTriangle.vn0 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 0]];
                hitTriangle.vn1 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 1]];
                hitTriangle.vn2 = mesh.vertexNormalCoords[mesh.vertexNormals[face, 2]];
                hitTriangle.uv0 = mesh.uvcoords[mesh.uvs[face, 0]];
                hitTriangle.uv1 = mesh.uvcoords[mesh.uvs[face, 1]];
                hitTriangle.uv2 = mesh.uvcoords[mesh.uvs[face, 2]];
                hitTriangle.normal = mesh.faceNormals[face];


                barycentricCoords = hitTriangle.GetBarycentricCoordinates(pt, out bool inTri);
                if (inTri)
                {
                    hit = new RayCastHit { hit = pt, barycentricCoordinates = barycentricCoords, collider = this, distance = t, normal = (smoothMesh ? hitTriangle.NormalAt(pt) : hitTriangle.normal), triangle = hitTriangle };
                    return true;
                }
            }
            hit = null;
            return false;
        }
        public virtual float DistanceToColliderSurface(Vector3 direction)
        {
            Triangle3 hitTriangle = new Triangle3();
            Vector3 pt;
            direction.Normalize();
            for (int face = 0; face < mesh.faces.GetLength(0); face++)
            {
                if (face == mesh.faces.GetLength(0) - 1)
                    Console.WriteLine("vertex: " + mesh.vertices[0]);
                //all normals should be negative
                float rayNormalDot = Vector3.Dot(mesh.faceNormals[face], direction);
                //Console.WriteLine(rayNormalDot);
                //>=
                if (rayNormalDot == 0)
                    continue;
                float planeConst = Vector3.Dot(mesh.faceNormals[face], mesh.vertices[mesh.faces[face, 0]]);
                float t = -(Vector3.Dot(mesh.faceNormals[face], object3D.transform.Location) + planeConst) / rayNormalDot;
                //Triangle is behind ray origin
                if (t < 0)
                    continue;

                pt = object3D.transform.Location + (t * direction);

                hitTriangle.v0 = mesh.vertices[mesh.faces[face, 0]];
                hitTriangle.v1 = mesh.vertices[mesh.faces[face, 1]];
                hitTriangle.v2 = mesh.vertices[mesh.faces[face, 2]];
                hitTriangle.normal = mesh.faceNormals[face];
                if (hitTriangle.InsideTriangle(pt))
                {
                    return t;
                }

            }
            throw new Exception("no hit");
        }
    }

    public class BoxCollider : Collider
    {
        public override Mesh mesh { get; protected set; }
        private Mesh staticMesh;
        private TransformUpdateDelegate updateMeshDelegate;

        public Transform originalTransform;

        public override void Awake()
        {
            staticMesh = new Mesh(@"..\..\Resources\Cube.obj");
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                staticMesh.vertices[i] = originalTransform.Rotation.RotateVector3(staticMesh.vertices[i] * originalTransform.Scale) + originalTransform.Location;
                if (i < staticMesh.vertexNormalCoords.Length)
                    staticMesh.vertexNormalCoords[i] = originalTransform.Rotation.RotateVector3(staticMesh.vertexNormalCoords[i] * originalTransform.Scale) + originalTransform.Location;
            }
            staticMesh.CalculateFaceNormals();

            if (updateMeshDelegate == null)
            {
                updateMeshDelegate = UpdateMesh;
            }
            mesh = new Mesh(staticMesh);
            object3D.transform.transformUpdate = updateMeshDelegate;
            updateMeshDelegate(TransformOps.All);
        }

        private void UpdateMesh(TransformOps ops)
        {
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                mesh.vertices[i] = object3D.transform.Rotation.RotateVector3(staticMesh.vertices[i] * object3D.transform.Scale) + object3D.transform.Location;
                if (i < staticMesh.vertexNormalCoords.Length)
                    mesh.vertexNormalCoords[i] = object3D.transform.Rotation.RotateVector3(staticMesh.vertexNormalCoords[i] * object3D.transform.Scale) + object3D.transform.Location;
            }

            if ((ops & TransformOps.Rotation) != 0)
            {
                mesh.CalculateFaceNormals();
            }
        }
    }

    public class SphereCollider : Collider
    {
        public override Mesh mesh { get { return null; } protected set { } }
        public float radius = 3;

        public override bool RayCast(Ray ray, out RayCastHit hit)
        {
            ray.direction.Normalize();
            hit = null;
            float t0, t1; // solutions for t if the ray intersects 

            // geometric solution
            Vector3 L = object3D.transform.Location - ray.origin;
            float tca = Vector3.Dot(L, ray.direction);
            if (tca < 0) return false;
            float d2 = Vector3.Dot(L, L) - tca * tca;
            if (d2 > radius) return false;
            float thc = (float)Math.Sqrt(radius - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            if (t0 > t1)
            {
                float temp = t1;
                t1 = t0;
                t0 = temp;
            }


            if (t0 < 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 < 0) return false; // both t0 and t1 are negative 
            }

            hit = new RayCastHit
            {
                collider = this,
                distance = t0,
                hit = ray.origin + t0 * ray.direction,
                normal = ray.origin + t0 * ray.direction - object3D.transform.Location,
            };
            return true;
        }
        public override float DistanceToColliderSurface(Vector3 direction)
        {
            return radius;
        }
    }

    [RequireComponent(typeof(MeshComponent))]
    public class MeshCollider : Collider
    {
        private MeshComponent mc;

        public override Mesh mesh {
            get { return mc.TransformedMesh; }
            protected set { }
        }

        public override void Awake()
        {
            mc = (MeshComponent)object3D.GetComponent(typeof(MeshComponent));
        }
    }

    public class AxisAllignedBoundingBox : Component
    {
        public AxisAllignedBoundingBox(Vector3 max, Vector3 min)
        {
            this.max = max; this.min = min;
        }

        public Vector3 max;
        public Vector3 min;

        public bool CollidingWith(AxisAllignedBoundingBox other)
        {
            return (max.x >= other.min.x && min.x <= other.max.x)
                && (max.y >= other.min.y && min.y <= other.max.y)
                && (max.z >= other.min.z && min.z <= other.max.z);
        }

        public RigidBody GetRigidBody()
        {
            return (RigidBody)object3D.GetComponent(typeof(RigidBody));
        }
    }

    [RequireComponent(typeof(Collider))]
    public class RigidBody : Component
    {
        public float mass = 1;
        public Vector3 velocity = Vector3.Zero;
        public Vector3 Acceleration { get { return (1 / mass) * netForce; } }
        public Vector3 netForce = Vector3.Zero;
        public bool updateInternally = true;
        public bool Static = false;

        public void AddForce(Vector3 force)
        {
            AddForce(force, ForceMode.Force);
        }
        public void AddForce(Vector3 force, ForceMode mode)
        {
            switch (mode)
            {
                case ForceMode.Velocity:
                    velocity += force;
                    break;
                case ForceMode.Acceleration:
                    netForce += mass * force;
                    break;
                case ForceMode.Force:
                    netForce += force;
                    break;
            }
        }
    }
    
}
