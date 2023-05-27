﻿using System.Collections.Generic;
using UnityEngine;

public class SubroutineDefinition
{
    public int EntrypointId { get; private set; }
    public Dictionary<string, ScrapsetTypes> LocalVariableDeclarations { get; private set; } = new Dictionary<string, ScrapsetTypes>();
    // a dictionary of local variable name -> gate ID, representing all gate instances of a local variable
    public Dictionary<string, List<int>> LocalVariableReferencers { get; private set; } = new Dictionary<string, List<int>>();

    int idCounter = 0;
    Dictionary<int, IGate> gates = new Dictionary<int, IGate>();

    // manages the gate I/O connections
    GateIORegistry gateIORegistry;
    // manages the program flow connections
    ProgramFlowRegistry programFlowRegistry;

    public SubroutineDefinition()
    {
        gateIORegistry = new GateIORegistry(this);
        programFlowRegistry = new ProgramFlowRegistry(this);
        EntrypointId = SpawnGate<Entrypoint>();
    }

    public IGate FindGateById(int id)
    {
        if (!gates.TryGetValue(id, out var gate))
        {
            return null;
        }

        return gate;
    }

    public int SpawnGate<T>() where T : IGate, new()
    {
        var gate = new T();
        gate.Id = idCounter++;
        gates.Add(gate.Id, gate);

        Debug.Log($"Spawning gate '{gate.Name}' with ID {gate.Id}");
        return gate.Id;
    }

    public void RemoveGate(int gateId)
    {
        var gate = FindGateById(gateId);

        if (gate == null)
        {
            throw new System.Exception($"Cannot remove gate with ID {gateId}: gate does not exist");
        }

        var name = gate.Name;
        var id = gate.Id;
        Debug.Log($"Removing gate '{name}' with ID {id}");

        // remove all links where this gate acts as an input or output
        gateIORegistry.RemoveAllInputOutputLinks(gateId);

        // remove program flows where this gate is the source or destination
        programFlowRegistry.RemoveAllProgramFlowLinks(gateId);

        gates[gateId] = null;

        Debug.Log($"Removed gate '{name}' with ID {id}");
    }

    public void CreateInputOutputLink(int inputGateId, string inputParameterName, int outputGateId, string outputParameterName)
    {
        gateIORegistry.CreateInputOutputLink(inputGateId, inputParameterName, outputGateId, outputParameterName);
    }

    public void CreateProgramFlowLink(int fromId, string flowName, int toId)
    {
        programFlowRegistry.CreateProgramFlowLink(fromId, flowName, toId);
    }

    public void RemoveProgramFlowLink(int fromId, string flowName)
    {
        programFlowRegistry.RemoveProgramFlowLink(fromId, flowName);
    }

    public void RemoveAllProgramFlowLinks(int gateId)
    {
        programFlowRegistry.RemoveAllProgramFlowLinks(gateId);
    }

    public void DeclareLocalVariable(string variableName, ScrapsetTypes scrapsetType)
    {
        if (LocalVariableDeclarations.ContainsKey(variableName))
        {
            throw new System.Exception($"Variable '{variableName}' has already been declared in this scope");
        }

        LocalVariableDeclarations.Add(variableName, scrapsetType);
    }

    public int SpawnVariable<T>(string variableName) where T : IGate, IVariable, new()
    {
        if (!LocalVariableDeclarations.ContainsKey(variableName))
        {
            throw new System.Exception($"Cannot spawn gate for variable '{variableName}': variable has not been declared");
        }

        var variableId = SpawnGate<T>();
        var newVariable = FindGateById(variableId) as IVariable;
        //newVariable.Reference = localVariableValues[variableName];
        newVariable.VariableName = variableName;

        if (!LocalVariableReferencers.ContainsKey(variableName))
        {
            LocalVariableReferencers.Add(variableName, new List<int>());
        }

        LocalVariableReferencers[variableName].Add(variableId);
        return variableId;
    }

    public bool HasInputLinks(int gateId)
    {
        return gateIORegistry.HasInputLinks(gateId);
    }

    public IList<KeyValuePair<string, GateLink>> GetInputLinks(int gateId)
    {
        return gateIORegistry.GetInputLinks(gateId);
    }

    public List<GateLink> GetOutputLinks(int gateId, string parameterName)
    {
        return gateIORegistry.GetOutputLinks(gateId, parameterName);
    }

    public ProgramFlow GetProgramFlowLink(IGate fromGate, string flowName)
    {
        return programFlowRegistry.GetProgramFlowLink(fromGate, flowName);
    }
}
