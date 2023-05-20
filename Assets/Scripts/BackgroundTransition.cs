using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float transitionStartSize = 9f;
    [SerializeField] float transitionEndSize = 22f;

    float smallBgAlpha = 1f;
    SpriteRenderer mySpriteRenderer;
    float prevCamSize;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        prevCamSize = cam.orthographicSize;

    }

    void Update()
    {
        if (cam.orthographicSize != prevCamSize)
        {
            Debug.Log("Running grid transition logic!");
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

        prevCamSize = cam.orthographicSize;
    }
}
