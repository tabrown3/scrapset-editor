using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 PanDirection { get; private set; }
    public Vector2 CursorPos { get; private set; }
    public bool IsPanningByDrag { get; private set; }
    public Vector2 InitDragPos { get; private set; }

    bool prevIsPanningByDrag;

    void Start()
    {

    }

    void Update()
    {

    }

    void OnPan(InputValue value)
    {
        PanDirection = value.Get<Vector2>();
    }

    void OnCursorPosition(InputValue value)
    {
        CursorPos = value.Get<Vector2>();
    }

    void OnActivatePanByDrag(InputValue value)
    {
        IsPanningByDrag = value.Get<float>() != 0f;
        if (IsPanningByDrag && !prevIsPanningByDrag)
        {
            InitDragPos = CursorPos;
        } else if (!IsPanningByDrag && prevIsPanningByDrag)
        {
            InitDragPos = Vector2.zero;
        }
        prevIsPanningByDrag = IsPanningByDrag;
    }
}
