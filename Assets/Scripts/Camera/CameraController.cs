using UnityEngine;

namespace Scrapset
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] float cameraPanSpeed = 3f;
        [SerializeField] InputManager inputManager;

        const string SCROLL_WHEEL_AXIS = "Mouse ScrollWheel";
        const float MIN_CAM_SIZE = 2f;
        const float MAX_CAM_SIZE = 65f;
        const float TOWARD_SCREEN = -1f;
        const float AWAY_FROM_SCREEN = 1f;

        UnityEngine.Camera cam;
        Vector3 prevPos;
        float prevSize;

        public bool PositionHasChanged { get; private set; }
        public bool SizeHasChanged { get; private set; }

        void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
            prevPos = cam.transform.position;
            prevSize = cam.orthographicSize;
        }

        void LateUpdate()
        {
            PanCamera();
            ZoomCamera();
            LimitCamera();
        }

        void LimitCamera()
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, MIN_CAM_SIZE, MAX_CAM_SIZE);
        }

        void PanCamera()
        {
            DragPan();
            ContinuousPan();
            TrackPositionChange();
        }

        // logic for panning by offset, like clicking middle mouse button and dragging
        void DragPan()
        {
            if (inputManager.IsPanningByDrag)
            {
                var delta = cam.ScreenToWorldPoint(inputManager.CursorPosScreen) - inputManager.InitDragPosWorld;
                cam.transform.position -= delta;
            }
        }

        // logic for panning by speed and direction, like holding an arrow key or tilting an analog stick
        void ContinuousPan()
        {
            if (inputManager.PanDirection != Vector2.zero)
            {
                var delta = inputManager.PanDirection * // PanDirection is a unit vector pointing in the world direction to move the camera
                    cam.orthographicSize * // we need the camera to translate slower when zoomed in and faster when zoomed out, so scaled to size
                    cameraPanSpeed * // this is an adjustable speed multiplier to make the pan speed UX feel right
                    Time.deltaTime; // makes camera pan framerate independent

                cam.transform.position += (Vector3)delta;
            }
        }

        void TrackPositionChange()
        {
            PositionHasChanged = cam.transform.position != prevPos;

            // MUST BE LAST LINE
            prevPos = cam.transform.position;
        }

        void ZoomCamera()
        {
            if (inputManager.ZoomDelta > 0f)
            {
                Zoom(TOWARD_SCREEN);
            } else if (inputManager.ZoomDelta < 0f)
            {
                Zoom(AWAY_FROM_SCREEN);
            }

            TrackSizeChange();
        }

        void Zoom(float direction)
        {
            // stores the mouse's pos in world coords before scaling
            var beforeZoomPos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var newSize = cam.orthographicSize + cam.orthographicSize * direction / 5f;
            cam.orthographicSize = Mathf.Clamp(newSize, MIN_CAM_SIZE, MAX_CAM_SIZE);
            var afterZoomPos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            // moves the camera such that the mouse appears not to have moved in world coords, which
            //  gives the "zoom toward and away from the mouse" effect
            cam.transform.position += beforeZoomPos - afterZoomPos;
        }

        void TrackSizeChange()
        {
            SizeHasChanged = cam.orthographicSize != prevSize;

            // MUST BE LAST LINE
            prevSize = cam.orthographicSize;
        }
    }
}
