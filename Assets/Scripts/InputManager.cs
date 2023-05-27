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
        new GameObject("Processor", typeof(Processor));
        var subroutineDefinition = new SubroutineDefinition();
        subroutineDefinition.DeclareLocalVariable("i", ScrapsetTypes.Number);

        /* First statement */
        var ifStatementId = GenerateIfStatement(subroutineDefinition);

        /* Second statement */
        var incrementStatementId = GenerateIncrementStatement(subroutineDefinition);

        subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", ifStatementId);
        subroutineDefinition.CreateProgramFlowLink(ifStatementId, "Then", incrementStatementId);
        subroutineDefinition.CreateProgramFlowLink(incrementStatementId, "Next", ifStatementId);
        // intentionally omitted the ELSE block

        var processor = FindObjectOfType<Processor>();
        processor.SubroutineDefinition = subroutineDefinition;
    }

    int GenerateIfStatement(SubroutineDefinition subroutineDefinition)
    {
        var ifGateId = subroutineDefinition.SpawnGate<IfGate>(); // spawn If statement
        var numberVariableId = subroutineDefinition.SpawnVariable<NumberVariableGate>("i"); // spawn Number Variable
        var constantValueId = subroutineDefinition.SpawnGate<ConstantValueGate>(); // spawn Constant Value
        var lessThanId = subroutineDefinition.SpawnGate<LessThanGate>(); // spawn Less Than

        subroutineDefinition.CreateInputOutputLink(lessThanId, "A", numberVariableId, "Out");
        subroutineDefinition.CreateInputOutputLink(lessThanId, "B", constantValueId, "Out");
        subroutineDefinition.CreateInputOutputLink(ifGateId, "Condition", lessThanId, "Out");

        return ifGateId;
    }

    int GenerateIncrementStatement(SubroutineDefinition subroutineDefinition)
    {
        var assignmentGateId = subroutineDefinition.SpawnGate<AssignmentGate>(); // spawn Number Assignment
        var numberVariableId = subroutineDefinition.SpawnVariable<NumberVariableGate>("i"); // spawn Number Variable
        var constantValueId = subroutineDefinition.SpawnGate<ConstantValueGate>(); // spawn Constant Value
        var addId = subroutineDefinition.SpawnGate<AddGate>(); // spawn Add

        subroutineDefinition.CreateInputOutputLink(addId, "A", constantValueId, "Out");
        subroutineDefinition.CreateInputOutputLink(addId, "B", numberVariableId, "Out");
        subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", addId, "Out");
        subroutineDefinition.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out");

        return assignmentGateId;
    }
}
