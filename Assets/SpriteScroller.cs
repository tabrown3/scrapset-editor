using UnityEngine;

public class SpriteScroller : MonoBehaviour
{
    // the image is 32x32 Unity units at 64 pixels-per-unit resolution scale (1, 1)
    private const int HALF_WIDTH = 16;
    private const int HALF_HEIGHT = 16;
    private float small_bg_alpha;

    void Update()
    {
        var camSize = Camera.main.orthographicSize;


        var bottomLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        var topRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

        var isOutOfBounds = bottomLeft.x < transform.position.x - HALF_WIDTH + 1 ||
            bottomLeft.y < transform.position.y - HALF_HEIGHT + 1 ||
            topRight.x > transform.position.x + HALF_WIDTH - 1 ||
            topRight.y > transform.position.y + HALF_HEIGHT - 1;

        if (isOutOfBounds)
        {
            Debug.Log("Out of BOUNDS!");
            transform.position = new Vector2(Mathf.RoundToInt(Camera.main.transform.position.x),
                Mathf.RoundToInt(Camera.main.transform.position.y));
        }
    }
}
