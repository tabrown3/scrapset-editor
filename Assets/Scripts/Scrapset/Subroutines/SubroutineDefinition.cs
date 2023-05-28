using System.Collections.Generic;
using UnityEngine;

public class SubroutineDefinition : IInputOutput
{
    public string Name { get; private set; }
    public int EntrypointId { get; private set; }
    // all variable names -> types that the user has declared as part of this subroutine declaration
    Dictionary<string, ScrapsetTypes> localVariableDeclarations = new Dictionary<string, ScrapsetTypes>();
    public IReadOnlyDictionary<string, ScrapsetTypes> LocalVariableDeclarations => localVariableDeclarations;
    // not only do subroutines contain gates with inputs/outputs, they have inputs/outputs themselves
    public Dictionary<string, ScrapsetTypes> InputParameters { get; private set; } = new Dictionary<string, ScrapsetTypes>();
    public Dictionary<string, ScrapsetTypes> OutputParameters { get; private set; } = new Dictionary<string, ScrapsetTypes>();
    public GenericTypeReconciler GenericTypeReconciler { get; private set; } = new GenericTypeReconciler();

    int idCounter = 0;
    // gate ID -> Gate instance
    Dictionary<int, IGate> gates = new Dictionary<int, IGate>();

    // manages the gate I/O connections
    GateIORegistry gateIORegistry;
    // manages the program flow connections
    ProgramFlowRegistry programFlowRegistry;
    // the manager storing this definition
    SubroutineManager subroutineManager;

    public SubroutineDefinition(string _name, SubroutineManager _subroutineManager)
    {
        gateIORegistry = new GateIORegistry(this);
        programFlowRegistry = new ProgramFlowRegistry(this);
        EntrypointId = CreateGate<Entrypoint>();
        subroutineManager = _subroutineManager;
        Name = _name;
    }

    public IGate FindGateById(int id)
    {
        if (!gates.TryGetValue(id, out var gate))
        {
            return null;
        }

        return gate;
    }

    public int CreateGate<T>() where T : IGate, new()
    {
        var gate = new T();
        return CreateGate(gate);
    }

    public int CreateGate<T>(T gate) where T : IGate
    {
        gate.Id = idCounter++;
        gates.Add(gate.Id, gate);

        Debug.Log($"Created gate '{gate.Name}' with ID {gate.Id}");
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
        Debug.Log($"Removed gate '{name}' with ID {id}");

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

    private int CreateVariableGate<T>(string variableName) where T : IGate, new()
    {
        var variableId = CreateGate<T>();
        var newVariable = FindGateById(variableId) as IIdentifiable;
        newVariable.Identifier = variableName;

        return variableId;
    }

    public void DeclareLocalVariable(string variableName, ScrapsetTypes scrapsetType)
    {
        if (LocalVariableDeclarations.ContainsKey(variableName))
        {
            throw new System.Exception($"Variable '{variableName}' has already been declared in this scope");
        }

        localVariableDeclarations.Add(variableName, scrapsetType);
    }

    public int CreateLocalVariableGate<T>(string variableName) where T : IGate, IIdentifiable, new()
    {
        if (!LocalVariableDeclarations.ContainsKey(variableName))
        {
            throw new System.Exception($"Cannot spawn gate for variable '{variableName}': variable has not been declared");
        }

        return CreateVariableGate<T>(variableName);
    }

    public void DeclareInputVariable(string parameterName, ScrapsetTypes scrapsetType)
    {
        if (InputParameters.ContainsKey(parameterName))
        {
            throw new System.Exception($"Input parameter '{parameterName}' has already been declared in this scope");
        }

        InputParameters.Add(parameterName, scrapsetType);
    }

    public int CreateInputVariableGate<T>(string inputName) where T : IGate, IIdentifiable, IReadable, new()
    {
        if (!InputParameters.ContainsKey(inputName))
        {
            throw new System.Exception($"Cannot spawn gate for input '{inputName}': input has not been declared");
        }

        return CreateVariableGate<T>(inputName);
    }

    public void DeclareOutputVariable(string parameterName, ScrapsetTypes scrapsetType)
    {
        if (OutputParameters.ContainsKey(parameterName))
        {
            throw new System.Exception($"Output parameter '{parameterName}' has already been declared in this scope");
        }

        OutputParameters.Add(parameterName, scrapsetType);
    }

    public int CreateOutputVariableGate<T>(string outputName) where T : IGate, IIdentifiable, IWritable, new()
    {
        if (!OutputParameters.ContainsKey(outputName))
        {
            throw new System.Exception($"Cannot spawn gate for output '{outputName}': output has not been declared");
        }

        return CreateVariableGate<T>(outputName);
    }

    public int CreateSubroutineGate(SubroutineDefinition subroutineDefinition)
    {
        var subroutineGate = new SubroutineGate(subroutineDefinition);
        return CreateGate(subroutineGate);
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

    public ScrapsetTypes GetInputParameter(string parameterName)
    {
        if (!InputParameters.TryGetValue(parameterName, out var parameterType))
        {
            return ScrapsetTypes.None;
        }

        return parameterType;
    }

    public ScrapsetTypes GetOutputParameter(string parameterName)
    {
        if (!OutputParameters.TryGetValue(parameterName, out var parameterType))
        {
            return ScrapsetTypes.None;
        }

        return parameterType;
    }
}
