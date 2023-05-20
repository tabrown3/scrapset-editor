using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [SerializeField] GameObject bgParent;
    BackgroundTransitionParent backgroundTransitionParent;
    SpriteRenderer mySpriteRenderer;

    void Start()
    {
        backgroundTransitionParent = bgParent.GetComponent<BackgroundTransitionParent>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        mySpriteRenderer.color = mySpriteRenderer.color.WithAlpha(backgroundTransitionParent.GetSmallBgAlpha());
    }
}
