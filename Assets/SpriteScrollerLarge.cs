using UnityEngine;

public class SpriteScrollerLarge : MonoBehaviour
{
    // this is basically exactly the same as the small version, but 8x larger
    // TODO: consolidate SpriteScroller and SpriteScrollerLarge
    private const int HALF_WIDTH = 128;
    private const int HALF_HEIGHT = 128;

    void Update()
    {
        var bottomLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        var topRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

        var isOutOfBounds = bottomLeft.x < transform.position.x - HALF_WIDTH + 8 ||
            bottomLeft.y < transform.position.y - HALF_HEIGHT + 8 ||
            topRight.x > transform.position.x + HALF_WIDTH - 8 ||
            topRight.y > transform.position.y + HALF_HEIGHT - 8;

        if (isOutOfBounds)
        {
            transform.position = new Vector2(Mathf.RoundToInt(Camera.main.transform.position.x / 8) * 8,
                Mathf.RoundToInt(Camera.main.transform.position.y / 8) * 8);
        }
    }
}
