using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 PanDirection { get; private set; }
    public Vector2 DraggingCursorPos { get; private set; }
    public bool IsPanningByDrag { get; private set; }

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

    void OnPanByDrag(InputValue value)
    {
        DraggingCursorPos = value.Get<Vector2>();
    }

    void OnActivatePanByDrag(InputValue value)
    {
        IsPanningByDrag = value.Get<float>() != 0f;
    }
}
