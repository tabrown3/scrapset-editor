using UnityEngine;

public class InfiniteGrid : MonoBehaviour
{
    [SerializeField] float tileWidth = 1f; // in Unity units
    [SerializeField] float tileHeight = 1f; // in Unity units
    [SerializeField] float gridWidth = 32f; // in Scrapset Editor tiles
    [SerializeField] float gridHeight = 32f; // in Scrapset Editor tiles
    [SerializeField] Camera cam;

    float halfWidth;
    float halfHeight;
    Vector3 prevCamPos;

    void Start()
    {
        halfWidth = gridWidth / 2f;
        halfHeight = gridHeight / 2f;
        prevCamPos = cam.transform.position;
    }

    void Update()
    {
        // if the camera hasn't moved, skip the bounds logic for performance
        if (cam.transform.position != prevCamPos)
        {
            var bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
            var topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));

            var isOutOfBounds = bottomLeft.x < transform.position.x - halfWidth + tileWidth || // give a buffer of 1 tile width
                bottomLeft.y < transform.position.y - halfHeight + tileHeight ||
                topRight.x > transform.position.x + halfWidth - tileWidth ||
                topRight.y > transform.position.y + halfHeight - tileHeight;

            if (isOutOfBounds)
            {
                transform.position = new Vector2(Mathf.RoundToInt(cam.transform.position.x / tileWidth) * tileWidth,
                    Mathf.RoundToInt(cam.transform.position.y / tileHeight) * tileHeight);
            }
        }

        prevCamPos = cam.transform.position;
    }
}
