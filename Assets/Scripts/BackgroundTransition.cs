using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float transitionStartSize = 9f;
    [SerializeField] float transitionEndSize = 22f;

    float smallBgAlpha = 1f;
    SpriteRenderer mySpriteRenderer;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        var camSize = cam.orthographicSize;
        var transitionSizeDiff = transitionEndSize - transitionStartSize;

        if (camSize < transitionStartSize)
        {
            smallBgAlpha = 1f;
        } else if (camSize >= transitionStartSize && camSize <= transitionEndSize)
        {
            smallBgAlpha = 1f - ((camSize - transitionStartSize) / transitionSizeDiff);
        } else // camSize > transitionEndSize
        {
            smallBgAlpha = 0f;
        }

        mySpriteRenderer.color = mySpriteRenderer.color.WithAlpha(smallBgAlpha);
    }
}
