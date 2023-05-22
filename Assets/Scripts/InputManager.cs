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
            GenerateTestProgram();
        }
        prevIsPanningByDrag = IsPanningByDrag;
    }

    void GenerateTestProgram()
    {
        var processor = FindObjectOfType<Processor>();

        /* First statement */
        (var firstStatementId, var numberVariableId) = GenerateTestStatement1(processor);

        /* Second statement */
        GenerateTestStatement2(processor, firstStatementId, numberVariableId);

        /* Manual run - will eventually happen in Processor.Update */
        processor.RunProgram();
    }

    (int, int) GenerateTestStatement1(Processor processor)
    {
        var assignmentGateId = processor.SpawnGate<NumberAssignmentGate>("Number Assignment"); // spawn Number Assignment
        processor.CreateProgramFlowLink(processor.EntrypointId, "Next", assignmentGateId); // program flow link Entrypoint -> Number Assignment
        var numberVariableId = processor.SpawnGate<NumberVariableGate>("Number Variable"); // spawn Number Variable
        processor.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out"); // I/O link Number Assignment -> Number Variable
        var addGateId = processor.SpawnGate<AddGate>("Add"); // spawn Add
        processor.CreateInputOutputLink(assignmentGateId, "In", addGateId, "Out"); // I/O link // Add -> Number Assignment
        var constantValueId = processor.SpawnGate<ConstantValueGate>("Constant Value"); // spawn Constant Value
        processor.CreateInputOutputLink(addGateId, "A", constantValueId, "Out"); // I/O link Constant Value -> Add
        //var constantValueId2 = processor.SpawnGate<ConstantValueGate>("Constant Value"); // spawn Constant Value
        processor.CreateInputOutputLink(addGateId, "B", constantValueId, "Out"); // I/O link Constant Value -> Add

        return (assignmentGateId, numberVariableId);
    }

    void GenerateTestStatement2(Processor processor, int prevStatementId, int numberVariableId)
    {
        var assignmentGateId = processor.SpawnGate<NumberAssignmentGate>("Number Assignment"); // spawn Number Assignment
        processor.CreateProgramFlowLink(prevStatementId, "Next", assignmentGateId); // program flow link previous Number Assignment -> this Number Assignment
        processor.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out"); // I/O link Number Assignment -> Number Variable
        var addGateId = processor.SpawnGate<AddGate>("Add"); // spawn Add
        processor.CreateInputOutputLink(assignmentGateId, "In", addGateId, "Out"); // I/O link // Add -> Number Assignment
        var constantValueId = processor.SpawnGate<ConstantValueGate>("Constant Value"); // spawn Constant Value
        processor.CreateInputOutputLink(addGateId, "A", constantValueId, "Out"); // I/O link Constant Value -> Add
        processor.CreateInputOutputLink(addGateId, "B", numberVariableId, "Out"); // I/O link Number Variable -> Add
    }
}
