using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const string SCROLL_WHEEL_AXIS = "Mouse ScrollWheel";
    private const float MIN_CAM_SIZE = 2f;
    private const float MAX_CAM_SIZE = 65f;
    private const int MIDDLE_MOUSE_BUTTON = 2;
    private const float TOWARD_SCREEN = -1f;
    private const float AWAY_FROM_SCREEN = 1f;

    private Vector3 mouseDownPos;
    private Camera cam;

    private void Start()
    {
        cam = transform.gameObject.GetComponent<Camera>();
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
        if (Input.GetMouseButtonDown(MIDDLE_MOUSE_BUTTON))
        {
            mouseDownPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(MIDDLE_MOUSE_BUTTON))
        {
            var delta = mouseDownPos - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += delta;
        }
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
    }

    void Zoom(float direction)
    {
        // stores the mouse's pos in world coords before scaling
        var beforeZoomPos = cam.ScreenToWorldPoint(Input.mousePosition);
        var newSize = cam.orthographicSize + cam.orthographicSize * direction / 5f;
        cam.orthographicSize = Mathf.Clamp(newSize, MIN_CAM_SIZE, MAX_CAM_SIZE); ;
        var afterZoomPos = cam.ScreenToWorldPoint(Input.mousePosition);
        // moves the camera such that the mouse appears not to have moved in world coords, which
        //  gives the "zoom toward and away from the mouse" effect
        cam.transform.position += beforeZoomPos - afterZoomPos;
    }
}
