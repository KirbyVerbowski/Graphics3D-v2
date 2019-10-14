# Graphics3D-v2
"A second attempt at 3D graphics"

Kirby Verbowski

10/13/2019:
- Changed focus and its now more of a game engine structured similarly to Unity.
- Object3D objects now have "components" which are things like bounding boxes, renderers, game scripts, etc.
- SceneManager class takes care of the runtime game environment
  - Maintaining state
  - Calls Start() and Update() on all components
- Rendering now happens more neatly, in the CameraComponent script
- The camera is now simply a derived type of Object3D which has extra variables

2018:
- Small-ish library/visual studio project which holds the basic necessities for rendering objects defined in .obj files.
- Only drawing calls are made using the SetPixel(x,y,color) method for a modified bitmap class.
- All other 3D vector arithmetic/projection/rendering is done in software
