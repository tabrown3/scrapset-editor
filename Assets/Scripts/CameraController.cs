using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float cameraPanSpeed = 3.5f;

    const string SCROLL_WHEEL_AXIS = "Mouse ScrollWheel";
    const float MIN_CAM_SIZE = 2f;
    const float MAX_CAM_SIZE = 65f;
    const float TOWARD_SCREEN = -1f;
    const float AWAY_FROM_SCREEN = 1f;

    Vector3 mouseDownPos;
    Camera cam;
    PlayerMovement playerMovement;
    Vector3 prevPos;
    float prevSize;
    bool prevDiscretePanIsActive;
    Vector3 initDragPos;

    public bool PositionHasChanged { get; private set; }
    public bool SizeHasChanged { get; private set; }

    void Start()
    {
        cam = GetComponent<Camera>();
        playerMovement = GetComponentInChildren<PlayerMovement>();
        prevPos = cam.transform.position;
        prevSize = cam.orthographicSize;
        prevDiscretePanIsActive = playerMovement.IsPanningByDrag;
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
        if (playerMovement.IsPanningByDrag && !prevDiscretePanIsActive)
        {
            initDragPos = cam.ScreenToWorldPoint(playerMovement.DraggingCursorPos);
        }
        prevDiscretePanIsActive = playerMovement.IsPanningByDrag;

        if (playerMovement.IsPanningByDrag)
        {
            var delta = cam.ScreenToWorldPoint(playerMovement.DraggingCursorPos) - initDragPos;
            cam.transform.position -= delta;
        }
    }

    // logic for panning by speed and direction, like holding an arrow key or tilting an analog stick
    void ContinuousPan()
    {
        if (playerMovement.PanDirection != Vector2.zero)
        {
            var delta = playerMovement.PanDirection * // PanDirection is a unit vector pointing in the world direction to move the camera
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
        if (Input.GetAxis(SCROLL_WHEEL_AXIS) > 0f)
        {
            Zoom(TOWARD_SCREEN);
        } else if (Input.GetAxis(SCROLL_WHEEL_AXIS) < 0f)
        {
            Zoom(AWAY_FROM_SCREEN);
        }

        TrackSizeChange();
    }

    void Zoom(float direction)
    {
        // stores the mouse's pos in world coords before scaling
        var beforeZoomPos = cam.ScreenToWorldPoint(Input.mousePosition);
        var newSize = cam.orthographicSize + cam.orthographicSize * direction / 5f;
        cam.orthographicSize = Mathf.Clamp(newSize, MIN_CAM_SIZE, MAX_CAM_SIZE);
        var afterZoomPos = cam.ScreenToWorldPoint(Input.mousePosition);
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
