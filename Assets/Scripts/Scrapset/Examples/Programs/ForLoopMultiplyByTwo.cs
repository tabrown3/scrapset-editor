using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Examples
{
    public class ForLoopMultiplyByTwo : MonoBehaviour, IProgram
    {
        public void Build()
        {
            // multiply-by-two is called as an expression from within top-level
            GenerateMultiplyByTwoSubroutine();
            GenerateForLoopSubroutine();
        }

        public Dictionary<string, ScrapsetValue> Run()
        {
            // find the subroutine manager
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            // get one of the stored subroutine definitions
            var definition = subroutineManager.GetDefinition("top-level");

            // build the subroutine's input object
            var subroutineInputs = new Dictionary<string, ScrapsetValue>();
            var inVal = new ScrapsetValue(ScrapsetTypes.Number) { Value = 3f };
            subroutineInputs.Add("InNumber", inVal);

            // create an instance (runner) for the definition
            var instance = new SubroutineInstance();
            instance.SubroutineDefinition = definition;

            // pass in the input args and execute the subroutine - hold on to the output
            var returnValues = instance.Execute(subroutineInputs);

            return returnValues;
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
            subroutineDefinition.CreateProgramFlowLink(incrementStatementId, "Next", ifStatementId);
            subroutineDefinition.CreateProgramFlowLink(ifStatementId, "Else", outputAssignmentStatementId);
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
            var subroutineNumberOutput = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return"); // spawn Number Variable
            var numberVariableId = subroutineDefinition.CreateLocalVariableGate<NumberVariableGate>("i"); // spawn Number Variable

            // find the multiply-by-two subroutine
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            var multiplyByTwoDefinition = subroutineManager.GetDefinition("multiply-by-two");

            // create a gate for the subroutine
            var subroutineGate = new SubroutineGate(multiplyByTwoDefinition);
            subroutineDefinition.CreateGate(subroutineGate);

            subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", subroutineGate.Id, "Return");
            subroutineDefinition.CreateInputOutputLink(subroutineGate.Id, "InNumber", numberVariableId, "Out");
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
}
