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
    Vector3 initCamPos;

    public bool PositionHasChanged { get; private set; }
    public bool SizeHasChanged { get; private set; }

    void Start()
    {
        cam = GetComponent<Camera>();
        playerMovement = GetComponentInChildren<PlayerMovement>();
        prevPos = cam.transform.position;
        prevSize = cam.orthographicSize;
        prevDiscretePanIsActive = playerMovement.DiscretePanIsActive;
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
        //if (Input.GetMouseButtonDown(MIDDLE_MOUSE_BUTTON))
        //{
        //    mouseDownPos = cam.ScreenToWorldPoint(Input.mousePosition);
        //}

        //if (Input.GetMouseButton(MIDDLE_MOUSE_BUTTON))
        //{
        //    var delta = mouseDownPos - cam.ScreenToWorldPoint(Input.mousePosition);
        //    cam.transform.position += delta;
        //}

        if (playerMovement.DiscretePanIsActive && !prevDiscretePanIsActive)
        {
            initDragPos = cam.ScreenToWorldPoint(playerMovement.PanDiscrete);
            initCamPos = cam.transform.position;
        }
        prevDiscretePanIsActive = playerMovement.DiscretePanIsActive;

        if (playerMovement.DiscretePanIsActive)
        {
            var delta = cam.ScreenToWorldPoint(playerMovement.PanDiscrete) - initDragPos;
            var newCamPos = cam.transform.position - new Vector3(initCamPos.x, initCamPos.y, 0) - delta;
            Debug.Log(newCamPos);
            cam.transform.position = newCamPos;
        }

        if (playerMovement.PanDirection != Vector2.zero)
        {
            var delta = playerMovement.PanDirection * // PanDirection is a unit vector pointing in the world direction to move the camera
                cam.orthographicSize * // we need the camera to translate slower when zoomed in and faster when zoomed out, so scaled to size
                cameraPanSpeed * // this is an adjustable speed multiplier to make the pan speed UX feel right
                Time.deltaTime; // makes camera pan framerate independent

            cam.transform.position += (Vector3)delta;
        }

        TrackPositionChange();
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
