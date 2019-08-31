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

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.z);   ///!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
            name = other.name;
        }
        public Mesh(string path)
        {
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



}
