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
        }
        prevIsPanningByDrag = IsPanningByDrag;
    }

    void OnBuild()
    {
        GenerateTestProgram();
    }

    void OnRun()
    {
        var processor = FindObjectOfType<Processor>();

        /* Manual run - will eventually happen in Processor.Update */
        processor.RunProgram();
    }

    void GenerateTestProgram()
    {
        var processor = FindObjectOfType<Processor>();
        processor.DeclareLocalVariable("i", ScrapsetTypes.Number);

        /* First statement */
        var ifStatementId = GenerateIfStatement(processor);

        /* Second statement */
        var incrementStatementId = GenerateIncrementStatement(processor);

        processor.CreateProgramFlowLink(processor.EntrypointId, "Next", ifStatementId);
        processor.CreateProgramFlowLink(ifStatementId, "Then", incrementStatementId);
        processor.CreateProgramFlowLink(incrementStatementId, "Next", ifStatementId);
        // intentionally omitted the ELSE block
    }

    int GenerateIfStatement(Processor processor)
    {
        var ifGateId = processor.SpawnGate<IfGate>("If"); // spawn If statement
        var numberVariableId = processor.SpawnVariable("i"); // spawn Number Variable
        var constantValueId = processor.SpawnGate<ConstantValueGate>("Constant Value"); // spawn Constant Value
        var lessThanId = processor.SpawnGate<LessThanGate>("Less Than"); // spawn Less Than

        processor.CreateInputOutputLink(lessThanId, "A", numberVariableId, "Out");
        processor.CreateInputOutputLink(lessThanId, "B", constantValueId, "Out");
        processor.CreateInputOutputLink(ifGateId, "Condition", lessThanId, "Out");

        processor.RemoveGate(constantValueId);

        return ifGateId;
    }

    int GenerateIncrementStatement(Processor processor)
    {
        var assignmentGateId = processor.SpawnGate<NumberAssignmentGate>("Number Assignment"); // spawn Number Assignment
        var numberVariableId = processor.SpawnVariable("i"); // spawn Number Variable
        var constantValueId = processor.SpawnGate<ConstantValueGate>("Constant Value"); // spawn Constant Value
        var addId = processor.SpawnGate<AddGate>("Add"); // spawn Add

        processor.CreateInputOutputLink(addId, "A", constantValueId, "Out");
        processor.CreateInputOutputLink(addId, "B", numberVariableId, "Out");
        processor.CreateInputOutputLink(assignmentGateId, "In", addId, "Out");
        processor.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out");

        return assignmentGateId;
    }
}
