using UnityEngine;

public class BackgroundTransitionParent : MonoBehaviour
{
    private float smallBgAlpha = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var camSize = Camera.main.orthographicSize;

        if (camSize < 12f)
        {
            smallBgAlpha = 1f;
        } else if (camSize >= 12f && camSize <= 18f)
        {
            smallBgAlpha = 1f - ((camSize - 12f) / 6f);
        } else // camSize > 18
        {
            smallBgAlpha = 0f;
        }
    }

    public float GetSmallBgAlpha()
    {
        return smallBgAlpha;
    }
}
