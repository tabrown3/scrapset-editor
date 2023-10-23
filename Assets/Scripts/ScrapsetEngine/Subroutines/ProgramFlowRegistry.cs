using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapset.Engine
{
    public class ProgramFlowRegistry
    {
        List<ProgramFlow> programFlows = new List<ProgramFlow>();
        SubroutineDefinition subroutineDefinition;

        public ProgramFlowRegistry(SubroutineDefinition _subroutineDefinition)
        {
            subroutineDefinition = _subroutineDefinition;
        }

        // establish program execution order by linking statements together
        public void CreateProgramFlowLink(int fromId, string flowName, int toId)
        {
            /*** START VALIDATION - DO NOT MODIFY STATE HERE ***/
            var result = ProgramFlowLinkRuleset.ValidateLinkCreation(subroutineDefinition, fromId, flowName, toId);

            if (result.HasErrors)
            {
                var error = result.Errors.First();
                throw new System.Exception(error.ToString());
            }

            var fromGate = result.ComputedValues.FromGate;
            var toGate = result.ComputedValues.ToGate;

            /*** END VALIDATION - START STATE MANIPULATION ***/

            var programFlow = new ProgramFlow()
            {
                FromGateId = fromGate.Id,
                FromFlowName = flowName,
                ToGateId = toGate.Id,
            };

            programFlows.Add(programFlow);

            Debug.Log($"Linked program flow from gate '{fromGate.GetType()}' to gate '{toGate.GetType()}'");
        }

        public void RemoveProgramFlowLink(int fromId, string flowName)
        {
            var programFlow = programFlows.Find(u => u.FromGateId == fromId && u.FromFlowName == flowName);
            if (programFlow == null)
            {
                throw new System.Exception($"Cannot remove program flow: no program flow links from path '{flowName}' of gate ID {fromId}");
            }

            programFlows.Remove(programFlow);

            Debug.Log($"Removed program flow link from gate ID {fromId} path '{flowName}'");
        }

        public void RemoveAllProgramFlowLinks(int gateId)
        {
            var flowsForGate = programFlows.Where(u => u.FromGateId == gateId || u.ToGateId == gateId).ToList();
            for (var i = flowsForGate.Count - 1; i >= 0; i--)
            {
                programFlows.Remove(programFlows[i]);
            }

            Debug.Log($"Removed all program flow links for gate ID {gateId}");
        }

        public ProgramFlow GetProgramFlowLink(IGate fromGate, string flowName)
        {
            return programFlows.FirstOrDefault(u => u.FromGateId == fromGate.Id && u.FromFlowName == flowName);
        }
    }
}
