using System.Collections.Generic;
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
        GenerateMultiplyByTwoSubroutine();
        GenerateForLoopSubroutine();
    }

    void OnRun()
    {
        // find the subroutine manager
        var subroutineManager = FindObjectOfType<SubroutineManager>();
        // get one of the stored subroutine definitions
        var definition = subroutineManager.GetDefinition("top-level");

        // build the subroutine's input object
        var subroutineInputs = new Dictionary<string, ScrapsetValue>();
        var inVal = new ScrapsetValue(ScrapsetTypes.Number) { Value = 4f };
        subroutineInputs.Add("InNumber", inVal);

        // create an instance (runner) for the definition
        var instance = new SubroutineInstance();
        instance.SubroutineDefinition = definition;

        // pass in the input args and execute the subroutine - hold on to the output
        var returnValues = instance.Execute(subroutineInputs);

        Debug.Log("Below are the subroutine's return values:");
        foreach (var kv in returnValues)
        {
            Debug.Log($"Identifier: '{kv.Key}', Value: {kv.Value.Value}, Type: {kv.Value.Type}");
        }
    }

    void GenerateForLoopSubroutine()
    {
        var subroutineManager = FindObjectOfType<SubroutineManager>();
        var subroutineDefinition = subroutineManager.DeclareSubroutine("top-level");

        // declare subroutine inputs and output
        subroutineDefinition.DeclareInputVariable("InNumber", ScrapsetTypes.Number);
        subroutineDefinition.DeclareOutputVariable("Return", ScrapsetTypes.Number);

        // declare local variables
        subroutineDefinition.DeclareLocalVariable("i", ScrapsetTypes.Number);


        /* First statement */
        var ifStatementId = GenerateIfStatement(subroutineDefinition);

        /* Second statement */
        var incrementStatementId = GenerateIncrementStatement(subroutineDefinition);

        /* Third statement */
        var outputAssignmentStatementId = GenerateOutputAssignmentStatement(subroutineDefinition);

        subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", ifStatementId);
        subroutineDefinition.CreateProgramFlowLink(ifStatementId, "Then", incrementStatementId);
        subroutineDefinition.CreateProgramFlowLink(incrementStatementId, "Next", outputAssignmentStatementId);
        subroutineDefinition.CreateProgramFlowLink(outputAssignmentStatementId, "Next", ifStatementId);
        // intentionally omitted the ELSE block
    }

    int GenerateIfStatement(SubroutineDefinition subroutineDefinition)
    {
        var ifGateId = subroutineDefinition.CreateGate<IfGate>(); // spawn If statement
        var numberVariableId = subroutineDefinition.CreateLocalVariableGate<NumberVariableGate>("i"); // spawn Number Variable
        var subroutineInputId = subroutineDefinition.CreateInputVariableGate<NumberVariableGate>("InNumber"); // spawn Constant Value
        var lessThanId = subroutineDefinition.CreateGate<LessThanGate>(); // spawn Less Than

        subroutineDefinition.CreateInputOutputLink(lessThanId, "A", numberVariableId, "Out");
        subroutineDefinition.CreateInputOutputLink(lessThanId, "B", subroutineInputId, "Out");
        subroutineDefinition.CreateInputOutputLink(ifGateId, "Condition", lessThanId, "Out");

        return ifGateId;
    }

    int GenerateIncrementStatement(SubroutineDefinition subroutineDefinition)
    {
        var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>(); // spawn Number Assignment
        var numberVariableId = subroutineDefinition.CreateLocalVariableGate<NumberVariableGate>("i"); // spawn Number Variable
        var constantValueId = subroutineDefinition.CreateGate<ConstantValueGate>(); // spawn Constant Value
        var addId = subroutineDefinition.CreateGate<AddGate>(); // spawn Add

        subroutineDefinition.CreateInputOutputLink(addId, "A", constantValueId, "Out");
        subroutineDefinition.CreateInputOutputLink(addId, "B", numberVariableId, "Out");
        subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", addId, "Out");
        subroutineDefinition.CreateInputOutputLink(numberVariableId, "In", assignmentGateId, "Out");

        return assignmentGateId;
    }

    int GenerateOutputAssignmentStatement(SubroutineDefinition subroutineDefinition)
    {
        var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>(); // spawn Number Assignment
        var constantValueId = subroutineDefinition.CreateGate<ConstantValueGate>(); // spawn Constant Value
        var subroutineNumberOutput = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return"); // spawn Number Variable

        // find the multiply-by-two subroutine
        var subroutineManager = FindObjectOfType<SubroutineManager>();
        var multiplyByTwoDefinition = subroutineManager.GetDefinition("multiply-by-two");

        // create a gate for the subroutine
        var subroutineGate = new SubroutineGate(multiplyByTwoDefinition);
        subroutineDefinition.CreateGate(subroutineGate);

        subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", subroutineGate.Id, "Return");
        subroutineDefinition.CreateInputOutputLink(subroutineGate.Id, "InNumber", constantValueId, "Out");
        subroutineDefinition.CreateInputOutputLink(subroutineNumberOutput, "In", assignmentGateId, "Out");

        return assignmentGateId;
    }

    // accepts Number input "InNumber", multiplies it by two, and returns in output "Return"
    void GenerateMultiplyByTwoSubroutine()
    {
        var subroutineManager = FindObjectOfType<SubroutineManager>();
        var subroutineDefinition = subroutineManager.DeclareSubroutine("multiply-by-two");

        // declare subroutine inputs and output
        subroutineDefinition.DeclareInputVariable("InNumber", ScrapsetTypes.Number);
        subroutineDefinition.DeclareOutputVariable("Return", ScrapsetTypes.Number);

        var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>(); // spawn Number Assignment
        var addId = subroutineDefinition.CreateGate<AddGate>(); // spawn Add
        var subroutineInputId = subroutineDefinition.CreateInputVariableGate<NumberVariableGate>("InNumber"); // spawn Constant Value
        var subroutineNumberOutput = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return"); // spawn Number Variable

        subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", addId, "Out");
        subroutineDefinition.CreateInputOutputLink(addId, "A", subroutineInputId, "Out");
        subroutineDefinition.CreateInputOutputLink(addId, "B", subroutineInputId, "Out");
        subroutineDefinition.CreateInputOutputLink(subroutineNumberOutput, "In", assignmentGateId, "Out");

        subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", assignmentGateId);
    }
}
