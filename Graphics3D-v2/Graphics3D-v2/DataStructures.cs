using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
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
            return (Math.Abs(v1.x - v2.x) < MathConst.EPSILON && Math.Abs(v1.y - v2.y) < MathConst.EPSILON && Math.Abs(v1.z - v2.z) < MathConst.EPSILON );
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

    //Mesh structure is immutable
    public class Mesh
    {
        public readonly Vector3[] vertices;
        public readonly int[][] edges;
        public readonly int[][] faces; //list of vetex indices       
        public readonly string name;
        public Vector3[] faceNormals;

        public void CalculateFaceNormals()
        {
            if (faces == null)
            {
                throw new Exception("This mesh has no faces!");
            }
            faceNormals = new Vector3[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i].Length < 3)
                {
                    throw new Exception("Faces must have at least 3 vertices!");
                }
                //Assumes the convention that vertices will construct a face in a counter-clockwise manner
                faceNormals[i] = Vector3.Cross(vertices[faces[i][1]] - vertices[faces[i][0]], vertices[faces[i][faces[i].Length - 1]] - vertices[faces[i][0]]).Normalized;
            }
        }

        public Mesh(Vector3[] vertices, int[][] faces, int[][] edges, string name)
        {
            this.vertices = new Vector3[vertices.Length];
            this.faces = new int[faces.Length][];
            this.edges = new int[edges.Length][];
            Array.Copy(vertices, this.vertices, vertices.Length);
            Array.Copy(faces, this.faces, faces.Length);
            Array.Copy(edges, this.edges, edges.Length);
            this.name = name;
        }
        public Mesh(Mesh other)
        {
            this.vertices = new Vector3[other.vertices.Length];
            this.faces = new int[other.faces.Length][];
            this.edges = new int[other.edges.Length][];
            Array.Copy(other.vertices, this.vertices, vertices.Length);
            Array.Copy(other.faces, this.faces, faces.Length);
            Array.Copy(other.edges, this.edges, edges.Length);
            this.name = other.name;
        }
        public Mesh(string path)
        {
            string line;
            string[] words;
            char[] lineCh;
            List<Vector3> vertices = new List<Vector3>();
            Vector3 temp = Vector3.Zero;
            List<List<int>> faces = new List<List<int>>();
            List<int[]> edges = new List<int[]>();
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

                    case "f":
                        StringBuilder sb = new StringBuilder();
                        int j = 0;
                        faces.Add(new List<int>());
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
                                j = 0;
                            }


                            faces[face].Add(int.Parse(sb.ToString()) - 1);
                            if ((int.Parse(sb.ToString()) - 1) < 0)
                            {
                                throw new Exception("Cannot parse negative-indexed files");
                            }
                            sb.Clear();
                        }
                        face++;
                        break;

                    case "o":
                        name = words[1];
                        break;
                }

            }
            stream.Close();

            this.vertices = vertices.ToArray();
            this.faces = new int[faces.Count][];
            for (int i = 0; i < faces.Count; i++)
            {
                this.faces[i] = faces[i].ToArray();
            }

            int current = 0;
            for (int i = 0; i < this.faces.Length; i++)
            {
                for (int j = 0; j < this.faces[i].Length - 1; j++)
                {
                    edges.Add(new int[2]);
                    edges[current][0] = this.faces[i][j];
                    edges[current][1] = this.faces[i][j + 1];
                    current++;
                }
                if (this.faces[i].Length != 0)
                {
                    edges.Add(new int[2]);
                    edges[current][0] = this.faces[i][0];
                    edges[current][1] = this.faces[i][faces[i].Count - 1];
                    current++;
                }
            }

            this.edges = edges.ToArray();
            CalculateFaceNormals();
        }
    }
}
