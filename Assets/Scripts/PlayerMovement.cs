using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 PanDirection { get; private set; }
    public Vector2 PanDiscrete { get; private set; }
    public Vector2 InitPanDiscrete { get; private set; }
    public bool DiscretePanIsActive { get; private set; }

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

    void OnPanDiscrete(InputValue value)
    {
        PanDiscrete = value.Get<Vector2>();
    }

    void OnActivateDiscretePan(InputValue value)
    {
        DiscretePanIsActive = value.Get<float>() != 0f;
    }
}
