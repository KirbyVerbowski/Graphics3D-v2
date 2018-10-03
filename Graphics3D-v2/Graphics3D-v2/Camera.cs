using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;

namespace Graphics3D_v2
{
    class Camera : Object3D
    {

        enum RenderMode { Wireframe, Solid }

        public List<Object3D> renderQueue = new List<Object3D>();
        public int renderWidth = 512;
        public int renderHeight = 256;
        private RenderMode renderMode = RenderMode.Wireframe;

        
    
    }
}
