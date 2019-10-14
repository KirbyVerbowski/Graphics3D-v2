using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
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

        public override void Start()
        {
            mr = (MeshRenderer)object3D.GetComponent(typeof(MeshRenderer));
        }

        public override void Update()
        {
            if (Input.KeyHeld(Keys.K))
            {
                Console.WriteLine(angle);
            }
            object3D.transform.Rotate(Vector3.UnitVectorZ, angle);
            color += 0xF;
            mr.albedo = Color.FromArgb(color);
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
