using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    public delegate void FragmentShaderDelegate(Fragment f);
    public delegate void VertexShaderDelegate(Vertex v);

    public static class MathConst
    {
        public const float DEG2RAD = 0.0174533f;
        public const float RAD2DEG = 57.2958f;
        public const float EPSILON = 1E-3f;
        public static Vector3 RandomOnUnitSphere()
        {
            Random rand = new Random();
            return (new Vector3(rand.Next(-360, 361), rand.Next(-360, 361), rand.Next(-360, 361))).Normalized;
        }
    }

    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin; this.direction = direction;
        }
    }

    public class RayCastHit
    {
        public Vector3 hit;
        public Collider collider;
        public Vector3 barycentricCoordinates;
        public float distance;
        public Vector3 normal;
        public Triangle3 triangle;
    }

    public struct Vector2
    {

        public float x;
        public float y;

        public float Magnitude {
            get {
                return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            }
            private set { }
        }
        public Vector2 Normalized {
            get {
                return this * (1 / Magnitude);
            }
        }

        public static Vector2 UnitVectorX {
            get {
                return new Vector2(1, 0);
            }
        }
        public static Vector2 UnitVectorY {
            get {
                return new Vector2(0, 1);
            }
        }
        public static Vector2 Zero {
            get {
                return new Vector2(0, 0);
            }
        }
        public static Vector2 One {
            get { return new Vector2(1, 1); }
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }


        public void Normalize()
        {
            this = this * (1 / this.Magnitude);
        }

        public override string ToString() => ("(" + x + ", " + y + ")");
        public override bool Equals(object obj) => (Vector2)obj == this;
        public override int GetHashCode() => this.x.GetHashCode() ^ this.y.GetHashCode();


        public static float Dot(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }
        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }
        public static float Angle(Vector2 v1, Vector2 v2)
        {
            float f = (float)(Math.Acos(Dot(v1, v2) / (v1.Magnitude * v2.Magnitude)));
            if (!float.IsNaN(f))
            {
                return f;
            }
            else
            {
                return 0;
            }

        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return (Math.Abs(v1.x - v2.x) < MathConst.EPSILON && Math.Abs(v1.y - v2.y) < MathConst.EPSILON);
        }
        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return (Math.Abs(v1.x - v2.x) >= MathConst.EPSILON || Math.Abs(v1.y - v2.y) >= MathConst.EPSILON);
        }
        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2 (v1.x - v2.x, v1.y - v2.y);
        }
        public static Vector2 operator *(Vector2 v1, float mag)
        {
            return new Vector2 (v1.x * mag, v1.y * mag);
        }
        public static Vector2 operator *(float mag, Vector2 v1)
        {
            return new Vector2 (v1.x * mag, v1.y * mag);
        }
        /// <summary>
        /// Component-wise vector multiplication
        /// </summary>
        public static Vector2 operator *(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }
        public static Vector2 operator -(Vector2 v1)
        {
            return v1 * (-1);
        }

        /// <summary>
        /// Get component by index (0 = x, 1 = y, 2 = z)
        /// </summary>
        public float this[int i] {
            get {
                switch (i)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (i)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

        }
        /*
        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.z);   ///!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }*/
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public float Magnitude {
            get {
                return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            }
            private set { }
        }
        public Vector3 Normalized {
            get {
                return this * (1 / Magnitude);
            }
            private set { }
        }

        public static Vector3 UnitVectorX {
            get {
                return new Vector3(1, 0, 0);
            }
            private set { }
        }
        public static Vector3 UnitVectorY {
            get {
                return new Vector3(0, 1, 0);
            }
            private set { }
        }
        public static Vector3 UnitVectorZ {
            get {
                return new Vector3(0, 0, 1);
            }
            private set { }
        }
        public static Vector3 Zero {
            get {
                return new Vector3(0, 0, 0);
            }
            private set { }
        }
        public static Vector3 One {
            get { return new Vector3(1, 1, 1); }
            private set { }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        public void Normalize()
        {
            this = this * (1 / this.Magnitude);
        }

        public override string ToString() => ("(" + x + ", " + y + ", " + z + ")");
        public override bool Equals(object obj) => (Vector3)obj == this;
        public override int GetHashCode() => this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;


        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            float x, y, z;
            x = v1.y * v2.z - v1.z * v2.y;
            y = v1.z * v2.x - v1.x * v2.z;
            z = v1.x * v2.y - v1.y * v2.x;
            return new Vector3(x, y, z);
        }
        public static float Angle(Vector3 v1, Vector3 v2)
        {
            float f = (float)(Math.Acos(Dot(v1, v2) / (v1.Magnitude * v2.Magnitude)));
            if (!float.IsNaN(f))
            {
                return f;
            }
            else
            {
                return 0;
            }

        }

        //Rotates clockwise apparently
        public static Vector3 Rotate(Vector3 vec, Vector3 axis, float angle)
        {
            Vector3 ax = axis.Normalized;
            return (float)Math.Cos(angle) * vec + (float)Math.Sin(angle) * Cross(axis, vec) + (1 - (float)Math.Cos(angle)) * Dot(axis, vec) * axis;
        }


        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return (Math.Abs(v1.x - v2.x) < MathConst.EPSILON && Math.Abs(v1.y - v2.y) < MathConst.EPSILON && Math.Abs(v1.z - v2.z) < MathConst.EPSILON);
        }
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return (Math.Abs(v1.x - v2.x) >= MathConst.EPSILON || Math.Abs(v1.y - v2.y) >= MathConst.EPSILON || Math.Abs(v1.z - v2.z) >= MathConst.EPSILON);
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static Vector3 operator *(Vector3 v1, float mag)
        {
            return new Vector3(v1.x * mag, v1.y * mag, v1.z * mag);
        }
        public static Vector3 operator *(float mag, Vector3 v1)
        {
            return new Vector3(v1.x * mag, v1.y * mag, v1.z * mag);
        }
        /// <summary>
        /// Component-wise vector multiplication
        /// </summary>
        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector3 operator -(Vector3 v1)
        {
            return v1 * (-1);
        }

        /// <summary>
        /// Get component by index (0 = x, 1 = y, 2 = z)
        /// </summary>
        public float this[int i] {
            get {
                switch (i)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (i)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

        }
    }

    public struct Quaternion
    {

        private float w, x, y, z;

        public float Magnitude {
            get {
                return (float)Math.Sqrt(Math.Pow(w, 2) + Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            }
            private set { }
        }
        public float SqrMagnitude {
            get {
                return (float)(Math.Pow(w, 2) + Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            }
            private set { }
        }

        public Quaternion Conjugate {
            get { return new Quaternion(w, -x, -y, -z); }
            private set { }
        }
        public bool IsUnitQuaternion {
            get { return (Math.Pow(w, 2) + Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)) - 1 < MathConst.EPSILON; }
            private set { }
        }

        public static Quaternion Identity {
            get { return new Quaternion(1, 0, 0, 0); }
            private set { }
        }

        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Quaternion(Vector3 axis, float angle)
        {
            axis.Normalize();
            this.w = (float)Math.Cos(-angle / 2);
            this.x = axis.x * (float)Math.Sin(-angle / 2);
            this.y = axis.y * (float)Math.Sin(-angle / 2);
            this.z = axis.z * (float)Math.Sin(-angle / 2);
        }
        

        public Vector3 RotateVector3(Vector3 original)
        {
            Quaternion temp = (this * new Quaternion(0, original.x, original.y, original.z) * this.Conjugate);
            return new Vector3(temp.x, temp.y, temp.z);
        }

        public void Normalize()
        {
            if (IsUnitQuaternion)
            {
                return;
            }
            float mag = (float)Math.Sqrt(Math.Pow(w, 2) + Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            w /= mag;
            x /= mag;
            y /= mag;
            z /= mag;
        }

        public override string ToString()
        {
            return "(" + w + ", " + x + "i, " + y + "j, " + z + "k)";
        }

        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            float w, x, y, z;
            w = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
            x = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
            y = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
            z = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w;
            return new Quaternion(w, x, y, z);
        }
    }

    //Mesh structure is immutable. Only handles triangles. Absolutely no quads or ngons.
    public class Mesh
    {
        public readonly Vector3[] vertices;
        public readonly Vector3[] vertexNormalCoords;
        public readonly int[,] edges;
        public readonly int[,] faces; //list of vetex indices       
        public readonly int[,] uvs;
        public readonly int[,] vertexNormals;
        public readonly Vector2[] uvcoords;
        public readonly string name;
        public Vector3[] faceNormals;
        public readonly Dictionary<string, int[]> groups;

        public void CalculateFaceNormals()
        {
            if (faces == null)
            {
                throw new Exception("This mesh has no faces!");
            }
            faceNormals = new Vector3[faces.GetLength(0)];
            for (int i = 0; i < faces.GetLength(0); i++)
            {
                //Assumes the convention that vertices will construct a face in a counter-clockwise manner
                faceNormals[i] = Vector3.Cross(vertices[faces[i,1]] - vertices[faces[i,0]], vertices[faces[i, 2]] - vertices[faces[i, 0]]).Normalized;
            }
        }

        public Mesh(Vector3[] vertices, int[,] faces, int[,] edges, string name)
        {
            this.groups = new Dictionary<string, int[]>();
            this.vertices = new Vector3[vertices.Length];
            this.faces = new int[faces.GetLength(0), 3];
            this.edges = new int[edges.GetLength(0), 2];
            for(int i = 0; i < this.faces.GetLength(0); i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    this.faces[i, j] = faces[i, j];
                }
            }
            for (int i = 0; i < this.edges.GetLength(0); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.edges[i, j] = edges[i, j];
                }
            }
            for(int i = 0; i < this.vertices.Length; i++)
            {
                this.vertices[i] = vertices[i];
            }
            this.name = name;
        }
        public Mesh(Mesh other)
        {
            vertices = other.vertices.Clone() as Vector3[];
            vertexNormalCoords = other.vertexNormalCoords.Clone() as Vector3[];
            vertexNormals = other.vertexNormals.Clone() as int[,];
            faces = other.faces.Clone() as int[,];
            edges = other.edges.Clone() as int[,];
            uvcoords = other.uvcoords.Clone() as Vector2[];
            uvs = other.uvs.Clone() as int[,];
            groups = new Dictionary<string, int[]>();
            foreach(string entry in other.groups.Keys)
            {
                groups.Add(entry.Clone() as string, other.groups[entry].Clone() as int[]);
            }
            name = other.name;
        }
        public Mesh(string path)
        {
            groups = new Dictionary<string, int[]>();
            string line;
            string[] words;
            char[] lineCh;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> vertexNormalCoords = new List<Vector3>();
            Vector3 temp = Vector3.Zero;
            Vector2 temp2 = Vector2.Zero;
            List<int[]> faces = new List<int[]>();
            List<int[]> edges = new List<int[]>();
            List<Vector2> uvcoords = new List<Vector2>();
            List<int[]> uvs = new List<int[]>();
            List<int[]> vertexNormals = new List<int[]>();
            List<int> groupVerts = new List<int>();
            int face = 0;
            System.IO.StreamReader stream = new System.IO.StreamReader(path);

            while ((line = stream.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                lineCh = line.ToCharArray();
                if (lineCh[0] == '#')
                {
                    continue;
                }

                Regex reg = new Regex(@"\s+");

                line = reg.Replace(line, " ");
                words = line.Split(' ');


                switch (words[0])
                {
                    case "v":
                        for (int i = 1; i < 4; i++)
                        {
                            temp[i - 1] = float.Parse(words[i]);
                        }
                        vertices.Add(temp);
                        break;

                    case "vn":
                        for (int i = 1; i < 4; i++)
                        {
                            temp[i - 1] = float.Parse(words[i]);
                        }
                        vertexNormalCoords.Add(temp);
                        break;

                    case "f":
                        StringBuilder sb = new StringBuilder();
                        int j = 0;
                        faces.Add(new int[3]);
                        uvs.Add(new int[3]);
                        vertexNormals.Add(new int[3]);
                        for (int i = 1; i < words.Length; i++)
                        {
                            if (!words[i].Contains("/"))
                            {
                                sb.Append(words[i]);
                            }
                            else
                            {
                                while (words[i].ElementAt(j) != '/')
                                {
                                    sb.Append(words[i].ElementAt(j));
                                    j++;
                                }

                            }

                            faces[face][i - 1] = (int.Parse(sb.ToString()) - 1);
                            if ((int.Parse(sb.ToString()) - 1) < 0)
                            {
                                throw new Exception("Cannot parse negative-indexed files");
                            }
                            sb.Clear();

                            j++; //Move cursor to the char after '/'
                            while (words[i].ElementAt(j) != '/')
                            {
                                sb.Append(words[i].ElementAt(j));
                                j++;
                            }
                            if (sb.Length > 0)
                            {
                                uvs[face][i - 1] = (int.Parse(sb.ToString()) - 1);
                                if ((int.Parse(sb.ToString()) - 1) < 0)
                                {
                                    throw new Exception("Cannot parse negative-indexed files");
                                }
                                sb.Clear();
                            }

                            j++; //Move cursor again
                            while (j < words[i].Length && words[i].ElementAt(j) != '/')
                            {
                                sb.Append(words[i].ElementAt(j));
                                j++;
                            }
                            if (sb.Length > 0)
                            {
                                vertexNormals[face][i - 1] = (int.Parse(sb.ToString()) - 1);
                                if ((int.Parse(sb.ToString()) - 1) < 0)
                                {
                                    throw new Exception("Cannot parse negative-indexed files");
                                }
                                sb.Clear();
                            }

                            j = 0;
                        }
                        face++;
                        break;

                    case "vt":
                        for (int i = 1; i < 3; i++)
                        {
                            temp2[i - 1] = float.Parse(words[i]);
                        }
                        uvcoords.Add(temp2);
                        break;

                    case "g":
                        groupVerts.Clear();
                        foreach(string s in words[2].Split('/'))
                        {
                            groupVerts.Add(int.Parse(s) - 1);
                        }
                        groups.Add(words[2], groupVerts.ToArray());
                        break;

                    case "o":
                        name = words[1];
                        break;
                }

            }
            stream.Close();

            this.vertices = vertices.ToArray();
            this.vertexNormalCoords = vertexNormalCoords.ToArray();
            if(vertexNormalCoords.Count == 0)
            {
                vertexNormalCoords.Add(Vector3.Zero);
            }
            if(uvcoords.Count == 0)
            {
                uvcoords.Add(Vector2.Zero);
            }
            this.uvcoords = uvcoords.ToArray();
            this.uvs = new int[faces.Count, 3];
            this.vertexNormals = new int[faces.Count, 3];
            this.vertexNormalCoords = vertexNormalCoords.ToArray();
            this.faces = new int[faces.Count, 3];
            for (int i = 0; i < faces.Count; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    this.faces[i, j] = faces[i][j];
                    this.uvs[i, j] = uvs[i][j];
                    this.vertexNormals[i, j] = vertexNormals[i][j];
                }
                
            }

            int current = 0;
            for (int i = 0; i < this.faces.GetLength(0); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    edges.Add(new int[2]);
                    edges[current][0] = this.faces[i, j];
                    edges[current][1] = this.faces[i, j + 1];
                    current++;
                }
                if (this.faces.GetLength(1) != 0)
                {
                    edges.Add(new int[2]);
                    edges[current][0] = this.faces[i, 0];
                    edges[current][1] = this.faces[i, 2];
                    current++;
                }
            }

            this.edges = new int[edges.Count, 2];
            for(int i = 0; i < edges.Count; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    this.edges[i, j] = edges[i][j];
                }
            }
            CalculateFaceNormals();
        }
    }

    public class Bone
    {
        public string name;
        public float length;
        public Bone parent;
        public List<Bone> children;
        public Transform transform;

        public void TransformUpdate(TransformOps ops)
        {

        }
    }

    public class Rig
    {
        public Bone root;

        public Rig(string path)
        {
            List<Bone> bones = new List<Bone>();
            List<string> parents = new List<string>();
            List<List<string>> childs = new List<List<string>>();
            Bone bone = null;
            System.IO.StreamReader stream = new System.IO.StreamReader(path);
            string line;
            string[] words;
            while ((line = stream.ReadLine()) != null)
            {
                if (line == "\n")
                    continue;
                Regex reg = new Regex(@"\s+");
                line = reg.Replace(line, " ");

                words = line.Split(' ');
                switch (words[0])
                {
                    case "name":
                        if(bone != null)
                        {
                            bones.Add(bone);
                        }
                        bone = new Bone();
                        bone.transform = new Transform();
                        bone.name = words[1];
                        break;

                    case "rot":
                        bone.transform.Rotation = new Quaternion(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]), float.Parse(words[4]));
                        break;

                    case "loc":
                        bone.transform.Location = new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                        break;

                    case "len":
                        bone.length = float.Parse(words[1]);
                        break;

                    case "parent":
                        parents.Add(words[1]);
                        break;

                    case "children":
                        List<string> children = new List<string>();
                        for(int i = 1; i < words.Length; i++)
                        {
                            children.Add(words[i]);
                        }
                        childs.Add(children);
                        break;
                }
            }
            stream.Close();
            for(int i = 0; i < bones.Count; i++)
            {
                //Set parents
                if(parents[i] == "None")
                {
                    root = bones[i];
                    bones[i].parent = null;
                }
                for(int j = 0; j < bones.Count; j++)
                {
                    if(bones[j].name == parents[i])
                    {
                        bones[i].parent = bones[j];
                        break;
                    }
                }

                //Set children
                bones[i].children = new List<Bone>();
                foreach(string name in childs[i])
                {
                    if(name == "None")
                    {
                        break;
                    }
                    for(int j = 0; j < bones.Count; j++)
                    {
                        if(bones[j].name == name)
                        {
                            bones[i].children.Add(bones[j]);
                            break;
                        }
                    }
                }
            }
        }

    }

    public class Fragment
    {
        public Vector2 uv;
        public System.Drawing.Color color;
        public float z;
        public int x, y;
        public Vector3 normal;
        public Vector3 faceNormal;
        public Camera camera;
        public Mesh thisMesh;
        public Triangle triangle;
    }

    public class Vertex
    {
        public Vector3 location;
        public Vector3 objectPos;
        public Camera camera;
    }

    [Flags]
    public enum TransformOps { Location = 1, Scale = 2, Rotation = 4, All = 7 }

    public delegate void TransformUpdateDelegate(TransformOps op);

    public class Transform
    {
        private Vector3 _Location;
        public Vector3 Location {
            get { return _Location; }
            set { _Location = value; transformUpdate(TransformOps.Location); }
        }
        private Vector3 _Scale;
        public Vector3 Scale {
            get { return _Scale; }
            set { _Scale = value; transformUpdate(TransformOps.Scale); }
        }
        private Quaternion _Rotation;
        public Quaternion Rotation {
            get { return _Rotation; }
            set { _Rotation = value; transformUpdate(TransformOps.Rotation); }
        }
        public TransformUpdateDelegate transformUpdate;

        public Transform(Vector3 location, Vector3 scale, Quaternion rotation)
        {
            _Location = location;
            _Scale = scale;
            _Rotation = rotation;
        }
        public Transform(Vector3 location, Quaternion rotation)
        {
            _Location = location;
            _Rotation = rotation;
        }
        public Transform(Vector3 location, Vector3 scale)
        {
            _Location = location;
            _Scale = scale;
            _Rotation = Quaternion.Identity;
        }
        public Transform(Vector3 location)
        {
            _Location = location;
            _Scale = Vector3.One;
            _Rotation = Quaternion.Identity;
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
            rotBy = rotBy.Conjugate;
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
            Quaternion rotBy = new Quaternion(axis, angle).Conjugate;
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

    public class Triangle
    {
        public Vector2 v0, v1, v2;
        public Vector2 uv0, uv1, uv2;
        public float z0, z1, z2;
        public Vector3 vn0, vn1, vn2;
        public Vector2[] Points {
            get { return new Vector2[] { v0, v1, v2 }; }
        }
        public Triangle(Vector2 v0, Vector2 v1, Vector2 v2, float z0, float z1, float z2, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector3 vn0, Vector3 vn1, Vector3 vn2)
        {
            this.v0 = v0; this.v1 = v1; this.v2 = v2;
            this.z0 = z0; this.z1 = z1; this.z2 = z2;
            this.uv0 = uv0; this.uv1 = uv1; this.uv2 = uv2;
            this.vn0 = vn0; this.vn1 = vn1; this.vn2 = vn2;
        }

        public Triangle()
        {
            v0 = Vector2.Zero; v1 = Vector2.Zero; v2 = Vector2.Zero;
            uv0 = Vector2.Zero; uv1 = Vector2.Zero; uv2 = Vector2.Zero;
            z0 = 0; z1 = 0; z2 = 0;
            vn0 = Vector3.Zero; vn1 = Vector3.Zero; vn2 = Vector3.Zero;
        }

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

        public Vector3 NormalAt(Vector3 barycentricCoords)
        {
            return vn0 * barycentricCoords.x + vn1 * barycentricCoords.y + vn2 * barycentricCoords.z;
        }

        public Vector3 NormalAt(Vector2 pt)
        {
            return NormalAt(GetBarycentricCoordinates(pt));
        }

        public Vector2 UVAt(Vector3 barycentricCoords)
        {
            return uv0 * barycentricCoords.x + uv1 * barycentricCoords.y + uv2 * barycentricCoords.z;
        }

        public Vector2 UVAt(Vector2 pt)
        {
            return UVAt(GetBarycentricCoordinates(pt));
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

    public class Triangle3
    {
        public Vector3 v0, v1, v2;
        public Vector2 uv0, uv1, uv2;
        public Vector3 vn0, vn1, vn2;
        public Vector3 normal;
        public Vector3[] Points {
            get { return new Vector3[] { v0, v1, v2 }; }
        }
        public Triangle3(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector3 vn0, Vector3 vn1, Vector3 vn2, Vector3 normal)
        {
            this.v0 = v0; this.v1 = v1; this.v2 = v2;
            this.uv0 = uv0; this.uv1 = uv1; this.uv2 = uv2;
            this.vn0 = vn0; this.vn1 = vn1; this.vn2 = vn2;
            this.normal = normal;
        }

        public Triangle3()
        {
            v0 = Vector3.Zero; v1 = Vector3.Zero; v2 = Vector3.Zero;
            uv0 = Vector2.Zero; uv1 = Vector2.Zero; uv2 = Vector2.Zero;
            vn0 = Vector3.Zero; vn1 = Vector3.Zero; vn2 = Vector3.Zero;
            normal = Vector3.Zero;
        }

        private float EdgeFunction(Vector3 a, Vector3 b, Vector3 c, out bool result)
        {
            Vector3 cross = Vector3.Cross(a - b, c - b);
            float fresult = -Vector3.Dot(normal, cross);
            //float fresult = -((c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x));
            result = fresult >= 0;
            return fresult;
        }
        private float EdgeFunction(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 cross = Vector3.Cross(a - b, c - b);
            float fresult = -Vector3.Dot(normal, cross);
            //float fresult = -((c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x));
            return fresult;
        }

        public bool InsideTriangle(Vector3 point)
        {
            EdgeFunction(v0, v1, point, out bool result1);
            EdgeFunction(v1, v2, point, out bool result2);
            EdgeFunction(v2, v0, point, out bool result3);
            return result1 && result2 && result3;
        }

        public Vector3 GetBarycentricCoordinates(Vector3 point)
        {
            float area = EdgeFunction(v0, v1, v2);
            float w0 = EdgeFunction(v1, v2, point);
            float w1 = EdgeFunction(v2, v0, point);
            float w2 = EdgeFunction(v0, v1, point);
            return new Vector3(w0 / area, w1 / area, w2 / area);
        }
        public Vector3 GetBarycentricCoordinates(Vector3 point, out bool inTri)
        {
            float area = EdgeFunction(v0, v1, v2);
            float w0 = EdgeFunction(v1, v2, point, out bool res1);
            float w1 = EdgeFunction(v2, v0, point, out bool res2);
            float w2 = EdgeFunction(v0, v1, point, out bool res3);
            inTri = res1 && res2 && res3;
            return new Vector3(w0 / area, w1 / area, w2 / area);
        }

        public Vector3 NormalAt(Vector3 pt)
        {
            Vector3 baryCoords = GetBarycentricCoordinates(pt);
            return vn0 * baryCoords.x + vn1 * baryCoords.y + vn2 * baryCoords.z;
        }

        public Vector2 UVAt(Vector3 pt)
        {
            Vector3 baryCoords = GetBarycentricCoordinates(pt);
            return uv0 * baryCoords.x + uv1 * baryCoords.y + uv2 * baryCoords.z;
        }

        public override string ToString()
        {
            return "(" + v0 + v1 + v2 + ")";
        }
    }

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

        public Bitmap ReadOnlyClone()
        {
            return Bitmap.Clone(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), Bitmap.PixelFormat);
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
            for (int i = 0; i < Bits.Length; i++)
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
}
