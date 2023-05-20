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
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
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
        Camera.main.orthographicSize -= Camera.main.orthographicSize / 5f;
    }

    void ZoomOut()
    {
        Camera.main.orthographicSize += Camera.main.orthographicSize / 5f;
    }
}
