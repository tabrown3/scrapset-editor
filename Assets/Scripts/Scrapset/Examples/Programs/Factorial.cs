using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Examples
{
    public class Factorial : MonoBehaviour, IProgram
    {
        public void Build()
        {
            BuildFactorial();
            BuildTopLevel();
        }

        public Dictionary<string, ScrapsetValue> Run()
        {
            // find the subroutine manager
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            // get one of the stored subroutine definitions
            var definition = subroutineManager.GetDefinition("top-level");

            // create an instance (runner) for the definition
            var instance = new SubroutineInstance();
            instance.SubroutineDefinition = definition;

            // pass in the input args and execute the subroutine - hold on to the output
            var returnValues = instance.Execute(new Dictionary<string, ScrapsetValue>());

            return returnValues;
        }

        void BuildFactorial()
        {
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            var subroutineDefinition = subroutineManager.DeclareSubroutine("factorial");
            subroutineDefinition.DeclareInputVariable("N", ScrapsetTypes.Number);
            subroutineDefinition.DeclareOutputVariable("Return", ScrapsetTypes.Number);

            // first statement
            var ifGateId = subroutineDefinition.CreateGate<IfGate>();
            var lessThanGateId = subroutineDefinition.CreateGate<LessThanGate>();
            var subroutineInputGateId = subroutineDefinition.CreateInputVariableGate<NumberVariableGate>("N");
            var constantValueGateId = subroutineDefinition.CreateGate<NumberConstantValueGate>();

            // if N < 1
            subroutineDefinition.CreateInputOutputLink(ifGateId, "Condition", lessThanGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(lessThanGateId, "A", subroutineInputGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(lessThanGateId, "B", constantValueGateId, "Out");

            // then Return = 1 return
            var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>();
            var subroutineOutputGateId = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return");

            subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", constantValueGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(subroutineOutputGateId, "In", assignmentGateId, "Out");

            // else Return = N * factorial(N-1) return
            var subroutineOutputGate2Id = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return");
            var assignmentGate2Id = subroutineDefinition.CreateGate<AssignmentGate>();
            var multiplyGateId = subroutineDefinition.CreateGate<MultiplyGate>();
            var factorialGateId = subroutineDefinition.CreateSubroutineGate(subroutineDefinition);
            var subtractGateId = subroutineDefinition.CreateGate<SubtractGate>();

            subroutineDefinition.CreateInputOutputLink(assignmentGate2Id, "In", multiplyGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(multiplyGateId, "A", subroutineInputGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(multiplyGateId, "B", factorialGateId, "Return");
            subroutineDefinition.CreateInputOutputLink(factorialGateId, "N", subtractGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(subtractGateId, "A", subroutineInputGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(subtractGateId, "B", constantValueGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(subroutineOutputGate2Id, "In", assignmentGate2Id, "Out");

            subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", ifGateId);
            subroutineDefinition.CreateProgramFlowLink(ifGateId, "Then", assignmentGateId);
            subroutineDefinition.CreateProgramFlowLink(ifGateId, "Else", assignmentGate2Id);
        }

        void BuildTopLevel()
        {
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            var subroutineDefinition = subroutineManager.DeclareSubroutine("top-level");
            subroutineDefinition.DeclareOutputVariable("Return", ScrapsetTypes.Number);

            var factorialSubroutine = subroutineManager.GetDefinition("factorial");

            var factorialGateId = subroutineDefinition.CreateSubroutineGate(factorialSubroutine);
            var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>();
            var constantValueGateId = subroutineDefinition.CreateGate(new NumberConstantValueGate(5f));
            var subroutineOutputGateId = subroutineDefinition.CreateOutputVariableGate<NumberVariableGate>("Return");

            subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", factorialGateId, "Return");
            subroutineDefinition.CreateInputOutputLink(factorialGateId, "N", constantValueGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(subroutineOutputGateId, "In", assignmentGateId, "Out");

            subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", assignmentGateId);
        }
    }
}
