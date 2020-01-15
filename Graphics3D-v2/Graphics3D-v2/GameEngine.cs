using System;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Graphics3D_v2
{
    class Driver
    {        
        [STAThread]
        static void Main(string[] args)
        {
            GameEngine engine = new GameEngine(512, 256);
            Thread.CurrentThread.Name = "Main";
            engine.Main();
            Application.Run(engine);   
        }        
    }

    class GameEngine : Form
    {

        public GameEngine(int windowWidth, int windowHeight)
        {
            FormClosed += (e, s) => { Environment.Exit(0); };
            ClientSize = new Size(windowWidth, windowHeight);
            GameManager.Initialize(windowWidth, windowHeight);
            Physics.Initialize();
            Time.OnPaintEvent = async (x) => { GameEngine_Paint2(); };
            DoubleBuffered = true;

            Input.SetForm(this);
        }

        public void Main()
        {

            SetUpPongGame();
            Time.Start();
        }

        private void SetUpPongGame()
        {// 0.7853f
            Camera maincamera = new Camera(new Transform(new Vector3(0, -25, 0)), 512, 256, 0.7853f);
            
            maincamera.AddComponent(new CameraComponent());


            Object3D player1 = new Object3D(new Transform(new Vector3(-11, 0, 0), new Vector3(0.5f, 1, 2)));
            player1.name = "Player1";
            player1.AddComponent(new MeshComponent { mesh = new Mesh(@"..\..\Resources\Cube.obj") });
            player1.AddComponent(new Paddle { player1 = true });
            player1.AddComponent(new MeshRenderer { albedo = Color.Red });

            Object3D player2 = new Object3D(new Transform(new Vector3(11, 0, 0), new Vector3(0.5f, 1, 2)));
            player2.name = "Player2";
            player2.AddComponent(new MeshComponent { mesh = new Mesh(@"..\..\Resources\Cube.obj") });
            player2.AddComponent(new Paddle { player1 = false });
            player2.AddComponent(new MeshRenderer { albedo = Color.Blue });

            Object3D ball = new Object3D(new Transform(new Vector3(0, 0, 0), new Vector3(0.35f, 0.35f, 0.35f)));
            ball.name = "Ball";
            ball.AddComponent(new MeshComponent { mesh = new Mesh(@"..\..\Resources\Cube.obj") });
            ball.AddComponent(new MeshRenderer { albedo = Color.Purple });
            ball.AddComponent(new PongBall { player1 = player1.transform, player2 = player2.transform });


            GameManager.AddObject(maincamera);
            GameManager.AddObject(player1);
            GameManager.AddObject(player2);
            GameManager.AddObject(ball);
        }

        private void GameEngine_Paint2()
        {
            using(Graphics g = CreateGraphics())
            {
                g.DrawImage(GameManager.canvas.Bitmap, new Point(0, 0));
            }
        }

    }
}
