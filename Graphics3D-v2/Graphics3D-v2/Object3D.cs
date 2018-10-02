using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    [Flags]
    public enum TransformOps { location = 1, scale = 2, rotation = 4, all = 7 }

    public delegate void TransformUpdateDelegate(TransformOps op);

    public class Transform
    {
        private Vector3 _Location;
        public Vector3 Location {
            get { return _Location; }
            set { _Location = value; transformUpdate(TransformOps.location); }
        }
        private Vector3 _Scale;
        public Vector3 Scale {
            get { return _Scale; }
            set { _Scale = value; transformUpdate(TransformOps.scale); }
        }
        private Quaternion _Rotation;
        public Quaternion Rotation {
            get { return _Rotation; }
            set { _Rotation = value; transformUpdate(TransformOps.rotation); }
        }
        public TransformUpdateDelegate transformUpdate;

        public Transform(Vector3 location, Vector3 scale, Quaternion rotation)
        {
            _Location = location;
            _Scale = scale;
            _Rotation = rotation;
            transformUpdate(TransformOps.all);
        }
        public Transform(Vector3 location, Vector3 scale)
        {
            _Location = location;
            _Scale = scale;
            _Rotation = Quaternion.Identity;
            transformUpdate(TransformOps.location | TransformOps.scale);
        }
        public Transform(Vector3 location)
        {
            _Location = location;
            _Scale = Vector3.One;
            _Rotation = Quaternion.Identity;
            transformUpdate(TransformOps.location);
        }
        public Transform()
        {
            _Location = Vector3.Zero;
            _Scale = Vector3.One;
            _Rotation = Quaternion.Identity;
        }

        public Vector3 Forward {
            get { return Rotation.RotateVector3(Vector3.UnitVectorY); }
            private set { }
        }
        public Vector3 Up {
            get { return Rotation.RotateVector3(Vector3.UnitVectorZ); }
            private set { }
        }
        public Vector3 Right {
            get { return Rotation.RotateVector3(Vector3.UnitVectorX); }
            private set { }
        }

        public void Rotate(Quaternion rotBy)
        {
            if (this.Rotation.SqrMagnitude < MathConst.EPSILON)
            {
                this.Rotation = rotBy * Quaternion.Identity;
            }
            else
            {
                this.Rotation = rotBy * this.Rotation;
            }
        }
        public void Rotate(Vector3 axis, float angle)
        {
            Quaternion rotBy = new Quaternion(axis, angle);
            if (this.Rotation.SqrMagnitude < MathConst.EPSILON)
            {
                this.Rotation = rotBy * Quaternion.Identity;
            }
            else
            {
                this.Rotation = rotBy * this.Rotation;
            }
        }
    }

    public class Object3D
    {
        public Transform transform;
        private Mesh mesh;
        public Mesh TransformedMesh {
            get; private set;
        }
        public bool noMesh = false;
        //Gizmo

        public Object3D(Transform transform, Mesh mesh)
        {
            this.mesh = mesh;
            TransformedMesh = new Mesh(mesh);
            this.transform = transform;
            this.transform.transformUpdate = UpdateMesh;
            this.transform.transformUpdate.Invoke(TransformOps.all);
        }
        public Object3D(Mesh mesh)
        {
            this.mesh = mesh;
            TransformedMesh = new Mesh(mesh);
            transform = new Transform();
            transform.transformUpdate = UpdateMesh;
            transform.transformUpdate.Invoke(TransformOps.all);
        }
        public Object3D(Transform transform)
        {
            noMesh = true;
            mesh = null;
            TransformedMesh = null;
            this.transform = transform; 
        }
        public Object3D()
        {
            noMesh = true;
            mesh = null;
            TransformedMesh = null;
            transform = new Transform();
        }

        private void UpdateMesh(TransformOps ops)
        {
            for (int i = 0; i < TransformedMesh.vertices.Length; i++)
            {
                TransformedMesh.vertices[i] = transform.Rotation.RotateVector3(mesh.vertices[i] * transform.Scale) + transform.Location;
            }

            if((ops & TransformOps.rotation) != 0)
            {
                TransformedMesh.CalculateFaceNormals();
            }
        }
    }
}
