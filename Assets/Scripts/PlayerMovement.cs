using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    void Start()
    {

    }

    void Update()
    {

    }

    void OnPan(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }
}
