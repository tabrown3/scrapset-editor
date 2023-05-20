using UnityEngine;

public class InfiniteGrid : MonoBehaviour
{
    [SerializeField] float tileWidth = 1f; // in Unity units
    [SerializeField] float tileHeight = 1f; // in Unity units
    [SerializeField] float gridWidth = 32f; // in tiles
    [SerializeField] float gridHeight = 32f; // in tiles
    [SerializeField] Camera cam;

    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        halfWidth = gridWidth / 2f;
        halfHeight = gridHeight / 2f;
    }

    void Update()
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
}
