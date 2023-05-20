using UnityEngine;

public class CameraLimiter : MonoBehaviour
{
    private Vector3 mouseDownPos;

    void LateUpdate()
    {
        PanCamera();
        ZoomCamera();
        LimitCamera();
    }

    void LimitCamera()
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 2f, 65f);
    }

    void PanCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
            mouseDownPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            var delta = mouseDownPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += delta;
        }
    }

    void ZoomCamera()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            ZoomIn();
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            ZoomOut();
        }
    }

    void ZoomIn()
    {
        var beforeZoomPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.orthographicSize -= Camera.main.orthographicSize / 5f;
        var afterZoomPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += beforeZoomPos - afterZoomPos;
    }

    void ZoomOut()
    {
        var beforeZoomPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.orthographicSize += Camera.main.orthographicSize / 5f;
        var afterZoomPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += beforeZoomPos - afterZoomPos;
    }
}
