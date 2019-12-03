using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;


namespace Graphics3D_v2
{

    [RequireComponent(typeof(CameraComponent))]
    public class Camera : Object3D
    {

        private float _horizFOV = 0.7853f;
        private int _renderHeight = 256;
        private int _renderWidth = 512;
        private float _projectionDistance = 1;


        public float nearClip = 0.1f;
        public float farClip = 100;

        public float aspectRatio {
            get { return (float)renderWidth / (float)renderHeight; }
        }
        public int renderWidth {
            get { return _renderWidth; }
            set { _renderWidth = value; UpdateRenderSettings(); }
        }
        public int renderHeight {
            get { return _renderHeight; }
            set { _renderHeight = value; UpdateRenderSettings(); }
        }
        public float projectionDistance {
            get { return _projectionDistance; }
            set { _projectionDistance = value; UpdateRenderSettings(); }
        }        
        public float horizFOV {
            get { return _horizFOV; }
            set { _horizFOV = value; UpdateRenderSettings(); }
        }
        public float vertFOV {
            get { return horizFOV / aspectRatio; }
        }

        public Vector3 coordTransform;
        public Vector3 normToScreen;
        public float screenNormCoeffX;
        public float screenNormCoeffZ;

        public Camera(Transform transform, int width, int height, float horizFOV) : base(transform)
        {
            renderHeight = height;
            renderWidth = width;
            this.horizFOV = horizFOV;
            coordTransform = new Vector3(renderWidth / 2, 0, renderHeight / 2);
            normToScreen = new Vector3(renderWidth / 2, 1, renderHeight / 2);
            screenNormCoeffX = 1 / (projectionDistance * (float)Math.Tan(horizFOV));
            screenNormCoeffZ = 1 / (projectionDistance * (float)Math.Tan(vertFOV));
            depthBuffer = new float[renderWidth * renderHeight];
        }
        private void UpdateRenderSettings()
        {
            coordTransform.x = renderWidth / 2;
            coordTransform.z = renderHeight / 2;
            normToScreen.x = coordTransform.x;
            normToScreen.z = coordTransform.z;
            screenNormCoeffX = 1 / (projectionDistance * (float)Math.Tan(horizFOV));
            screenNormCoeffZ = 1 / (projectionDistance * (float)Math.Tan(vertFOV));

            depthBuffer = new float[renderWidth * renderHeight];
        }

        
        public float[] depthBuffer;

        public Vertex vertex = new Vertex();
        public Fragment fragment = new Fragment();
        public Vector3 e1, e2, e3; //Normalized between -1 and 1
        public Vector3 camPos, vert1, vert2, vert3;
        public Vector3 beforeClip1, beforeClip2, beforeClip3;

        public Triangle projectedTriangle = new Triangle();
        public List<Vector3> drawPoints = new List<Vector3>();    
    }

}
