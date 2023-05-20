using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 PanDirection { get; private set; }

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
}
