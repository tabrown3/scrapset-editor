using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 PanDirection { get; private set; }
    public float ZoomDelta { get; private set; }
    public Vector2 CursorPosScreen { get; private set; }
    public Vector3 CursorPosWorld { get; private set; }
    public bool IsPanningByDrag { get; private set; }
    public Vector3 InitDragPosWorld { get; private set; }

    [SerializeField] Camera cam;

    bool prevIsPanningByDrag;


    void OnPan(InputValue value)
    {
        PanDirection = value.Get<Vector2>();
    }

    void OnZoom(InputValue value)
    {
        ZoomDelta = value.Get<Vector2>().y;
    }

    void OnCursorPosition(InputValue value)
    {
        var screenPos = value.Get<Vector2>();
        CursorPosScreen = screenPos;
        CursorPosWorld = cam.ScreenToWorldPoint(screenPos);
    }

    void OnActivatePanByDrag(InputValue value)
    {
        IsPanningByDrag = value.Get<float>() != 0f;
        if (IsPanningByDrag && !prevIsPanningByDrag)
        {
            InitDragPosWorld = CursorPosWorld;
            FakeMethod();
        }
        prevIsPanningByDrag = IsPanningByDrag;
    }

    void FakeMethod()
    {
        var processor = FindObjectOfType<Processor>();
        var assignmentGateId = processor.SpawnGate<AssignmentGate>("Assignment");
        processor.CreateProgramFlowLink(processor.EntrypointId, "Next", assignmentGateId);
        //var numberVariableId = processor.SpawnGate<NumberVariableGate>("Number Variable");
        //processor.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out");

        var addGateId = processor.SpawnGate<AddGate>("Add");
        //processor.CreateInputOutputLink(assignmentGateId, "In", addGateId, "Out");
        var constantValueId = processor.SpawnGate<ConstantValueGate>("Constant Value");
        processor.CreateInputOutputLink(addGateId, "A", constantValueId, "Out");
    }
}
