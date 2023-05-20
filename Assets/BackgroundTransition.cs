using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [SerializeField] GameObject bgParent;
    SpriteScroller bgSpriteScroller;
    SpriteRenderer mySpriteRenderer;

    void Start()
    {
        bgSpriteScroller = bgParent.GetComponent<SpriteScroller>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        mySpriteRenderer.color = mySpriteRenderer.color.WithAlpha(bgSpriteScroller.GetSmallBgAlpha());
    }
}
