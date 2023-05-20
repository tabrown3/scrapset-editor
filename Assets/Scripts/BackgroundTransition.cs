using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float transitionStartSize = 9f;
    [SerializeField] float transitionEndSize = 22f;

    SpriteRenderer mySpriteRenderer;
    CameraController camController;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        camController = cam.GetComponent<CameraController>();

    }

    void Update()
    {
        if (camController.SizeHasChanged)
        {
            var camSize = cam.orthographicSize;
            var transitionSizeDiff = transitionEndSize - transitionStartSize;

            float smallBgAlpha;
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
}
