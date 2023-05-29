using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        var fromGate = subroutineDefinition.GetGateById(fromId);
        if (fromGate == null)
        {
            throw new System.Exception($"Gate with ID {fromId} not found");
        }

        var toGate = subroutineDefinition.GetGateById(toId);
        if (toGate == null)
        {
            throw new System.Exception($"Gate with ID {toId} not found");
        }

        IStatement fromStatement = fromGate as IStatement;
        if (fromStatement == null)
        {
            throw new System.Exception($"Gate with ID {fromId} does not implement IStatement");
        }

        IStatement toStatement = toGate as IStatement;
        if (toStatement == null)
        {
            throw new System.Exception($"Gate with ID {toId} does not implement IStatement");
        }

        if (!fromStatement.OutwardPaths.Contains(flowName))
        {
            throw new System.Exception($"Gate with ID {fromId} does not have outward path of '${flowName}'");
        }

        var programFlow = new ProgramFlow()
        {
            FromGateId = fromGate.Id,
            FromFlowName = flowName,
            ToGateId = toGate.Id,
        };

        programFlows.Add(programFlow);

        Debug.Log($"Linked program flow from gate '{fromGate.Name}' to gate '{toGate.Name}'");
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
        var flowsForGate = programFlows.Where(u => u.FromGateId == gateId || u.ToGateId == gateId);
        foreach (var programFlow in flowsForGate)
        {
            programFlows.Remove(programFlow);
        }

        Debug.Log($"Removed all program flow links for gate ID {gateId}");
    }

    public ProgramFlow GetProgramFlowLink(IGate fromGate, string flowName)
    {
        return programFlows.FirstOrDefault(u => u.FromGateId == fromGate.Id && u.FromFlowName == flowName);
    }
}
