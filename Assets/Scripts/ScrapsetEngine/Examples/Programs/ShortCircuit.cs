using Scrapset.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Examples
{
    public class ShortCircuit : MonoBehaviour, IProgram
    {
        public void Build()
        {
            var subroutineManager = FindObjectOfType<SubroutineManager>();
            var subroutineDefinition = subroutineManager.DeclareSubroutine("top-level");
            subroutineDefinition.DeclareOutputVariable("Return", ScrapsetTypes.Bool);

            var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>();
            var falseValueGateId = subroutineDefinition.CreateGate<BoolConstantValueGate>();
            var trueValueGateId = subroutineDefinition.RegisterGate(new BoolConstantValueGate(true));
            var andGateId = subroutineDefinition.CreateGate<AndGate>();
            var boolOutputId = subroutineDefinition.CreateOutputVariableGate<BoolVariableGate>("Return");

            subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", andGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(andGateId, "A", falseValueGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(andGateId, "B", trueValueGateId, "Out");
            subroutineDefinition.CreateInputOutputLink(boolOutputId, "In", assignmentGateId, "Out");

            subroutineDefinition.CreateProgramFlowLink(subroutineDefinition.EntrypointId, "Next", assignmentGateId);
        }

        public Dictionary<string, ScrapsetValue> Run()
        {
            // find the subroutine manager
            var subroutineManager = FindObjectOfType<SubroutineManager>();

            var returnValues = subroutineManager.CallSubroutine("top-level", new Dictionary<string, ScrapsetValue>());
            return returnValues;
        }
    }
}
