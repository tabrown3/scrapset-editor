using UnityEngine;

public class BackgroundTransitionParent : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float transitionStartSize = 12f;
    [SerializeField] float transitionEndSize = 18f;

    float smallBgAlpha = 1f;
    float transitionSizeDiff;

    void Start()
    {
        transitionSizeDiff = transitionEndSize - transitionStartSize;
    }

    void Update()
    {
        var camSize = cam.orthographicSize;

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
    }

    public float GetSmallBgAlpha()
    {
        return smallBgAlpha;
    }
}
