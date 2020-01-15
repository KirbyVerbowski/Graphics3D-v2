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
        SphereCollider sc;
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

            sc = (SphereCollider)object3D.GetComponent(typeof(SphereCollider));
        }

        public override void OnTriggerEnter(Collider other)
        {
            Console.WriteLine("Collision enter");
        }

        public override void Update()
        {

            //Console.WriteLine("update");
            if (Input.KeyHeld(Keys.K))
            {
                Console.WriteLine(angle);
                object3D.transform.Location += new Vector3(1, 0, 0);
            }
            object3D.transform.Rotate(Vector3.UnitVectorZ, angle);
            sc.radius -= 0.05f;
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

    public class PongBall : Component
    {
        public Transform player1;
        public Transform player2;

        private Vector3 velocity = Vector3.UnitVectorZ;
        public float speed = 0.35f;
        public float speedIncrease = 0.1f;
        public float height = 5f;
        public float width = 10.4f;
        public float paddleEpsilon = 0.15f;

        Random r = new Random();

        public override void Update()
        {
            if (Input.KeyHeld(Keys.Space))
            {
                velocity = Mathf.RandomOnUnitSphere();
                velocity.y = 0;
                velocity.Normalize();

                while(Math.Abs(velocity.x) <= 0.25)
                {
                    velocity = Mathf.RandomOnUnitSphere();
                    velocity.y = 0;
                    velocity.Normalize();
                }
                speed = 0.35f;
                object3D.transform.Location = Vector3.Zero;
            }

            object3D.transform.Location += speed * velocity;

            if(object3D.transform.Location.z >= height || object3D.transform.Location.z <= -height)
            {
                velocity.z = -velocity.z;
            }

            if(object3D.transform.Location.x >= width)
            {
                if(object3D.transform.Location.z <= player2.Location.z + player2.Scale.z + paddleEpsilon &&
                    object3D.transform.Location.z >= player2.Location.z - player2.Scale.z - paddleEpsilon)
                {
                    velocity.x = -velocity.x;
                    velocity.z += (float)r.NextDouble();
                    velocity.Normalize();
                    speed += speedIncrease;
                }
                else
                {
                    speed = 0;
                    object3D.transform.Location = Vector3.Zero;
                }
            }

            if (object3D.transform.Location.x <= -width)
            {
                if (object3D.transform.Location.z <= player1.Location.z + player1.Scale.z + paddleEpsilon &&
                    object3D.transform.Location.z >= player1.Location.z - player1.Scale.z - paddleEpsilon)
                {
                    velocity.x = -velocity.x;
                    velocity.z += (float)r.NextDouble();
                    velocity.Normalize();
                    speed += speedIncrease;
                }
                else
                {
                    speed = 0;
                    object3D.transform.Location = Vector3.Zero;
                }
            }
        }

    }

    public class Paddle : Component
    {
        public bool player1 = true;
        public float height = 4f;

        public override void Start()
        {
            //object3D.transform.Location = Vector3.Zero;
        }

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        public override void Update()
        {
            if (player1)
            {
                if (Input.KeyHeld(Keys.A) && object3D.transform.Location.z <= height)
                {
                    object3D.transform.Location += 0.5f * Vector3.UnitVectorZ;
                }
                else if (Input.KeyHeld(Keys.Z) && object3D.transform.Location.z >= -height)
                {
                    object3D.transform.Location -= 0.5f * Vector3.UnitVectorZ;
                }
            }
            else
            {
                if (Input.KeyHeld(Keys.Up) && object3D.transform.Location.z <= height)
                {
                    object3D.transform.Location += 0.5f * Vector3.UnitVectorZ;
                }
                else if (Input.KeyHeld(Keys.Down) && object3D.transform.Location.z >= -height)
                {
                    object3D.transform.Location -= 0.5f * Vector3.UnitVectorZ;
                }
            }
        }
    }

    
}
