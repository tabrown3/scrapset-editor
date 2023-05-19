using UnityEngine;

public class SpriteScroller : MonoBehaviour
{
    Material material;
    Vector3 prevPos;
    Vector3 originalPos;

    void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        // this magic half-scale that I don't understand allows the background material to remain stationary with respect to game objects
        //  when its offset changes to the previous camera world position
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        prevPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        originalPos = prevPos;
    }

    void Update()
    {
        Debug.Log(prevPos - originalPos);
        material.mainTextureOffset = prevPos - originalPos;
        prevPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
    }
}
