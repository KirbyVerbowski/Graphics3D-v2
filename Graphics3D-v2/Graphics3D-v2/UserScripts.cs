using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MyScript : Component
    {
        public float angle = 0.15f;
        MeshRenderer mr;
        int color = Color.Black.ToArgb();
        int i = 0;

        public override void Start()
        {
            //throw new NullReferenceException();
            mr = (MeshRenderer)object3D.GetComponent(typeof(MeshRenderer));
            //StartCoroutine(MyFirstCoroutine());
            Ray ray = new Ray(new Vector3(0.5f, -25, 0), Vector3.UnitVectorY);
            if (Physics.RayCast(ray, out RayCastHit hit))
            {
                Console.WriteLine("obj: " + hit.collider.object3D.name);
                Console.WriteLine("hit: " + hit.hit);
            }
                //if(Physics.RayCastAll(ray, out RayCastHit[] hits)){
                //    foreach(RayCastHit hit in hits)
                //    {

                //        Console.WriteLine(hit.collider.object3D.name);
                //        Console.WriteLine("normal: " + hit.normal);
                //        Console.WriteLine("pos: " + hit.hit);
                //    }
                //}
                MeshCollider c = (MeshCollider)object3D.GetComponent(typeof(MeshCollider));
            Console.WriteLine(c.DistanceToColliderSurface(Vector3.UnitVectorX));
        }

        public override void Update()
        {

            //Console.WriteLine("update");
            if (Input.KeyHeld(Keys.K))
            {
                Console.WriteLine(angle);
            }
            object3D.transform.Rotate(Vector3.UnitVectorZ, angle);
            //color += 0xF;
            //mr.albedo = Color.FromArgb(color);
        }

        public IEnumerator MyFirstCoroutine()
        {
            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                yield return null;
            }
        }

        public override void FixedUpdate()
        {
            //throw new NullReferenceException("yeet");
            //Console.WriteLine("fiexdupdate");
        }
    }

    public class SecondScript : Component
    {
        public Object3D firstObj;

        private MyScript firstScript;
        public override void Start()
        {
            firstScript = (MyScript)firstObj.GetComponent(typeof(MyScript));
            firstScript.angle = 0;
        }

        public override void Update()
        {
            if (Input.KeyHeld(Keys.Space))
            {
                firstScript.angle = 0.15f;
                object3D.transform.Rotate(Vector3.UnitVectorZ, -0.15f);
            }
            else
            {
                firstScript.angle = 0;
            }
        }
    }

    public class Paddle : Component
    {
        public bool player1;

        public override void Start()
        {
            object3D.transform.Location = Vector3.Zero;
        }

        public override void Update()
        {
            if (player1)
            {
                if (Input.KeyHeld(Keys.A))
                {
                    //kObject.
                }
            }
            else
            {

            }
        }
    }

    
}
