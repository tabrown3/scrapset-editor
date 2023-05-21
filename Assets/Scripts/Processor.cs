using System.Collections.Generic;
using UnityEngine;

public class Processor : MonoBehaviour
{
    int idCounter = 0;
    Dictionary<int, IExecutable> gates = new Dictionary<int, IExecutable>();
    Dictionary<int, GameObject> gateObjects = new Dictionary<int, GameObject>();
    Dictionary<int, GateLink> linksByOutputGate = new Dictionary<int, GateLink>();
    Dictionary<int, GateLink> linksByInputGate = new Dictionary<int, GateLink>();
    List<ProgramFlow> programFlows = new List<ProgramFlow>();

    // Start is called before the first frame update
    void Start()
    {
        SpawnGate<Entrypoint>("Entrypoint");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int SpawnGate<T>(string name) where T : MonoBehaviour, IExecutable, new()
    {
        var tempGameObj = new GameObject(name, typeof(T));
        var gate = tempGameObj.GetComponent<IExecutable>();
        gate.Id = idCounter++;
        Debug.Log(gate.Id);
        gates.Add(gate.Id, gate);
        gateObjects.Add(gate.Id, tempGameObj);
        tempGameObj.transform.parent = transform;

        return gate.Id;
    }

    public void LinkGates(int outputGateId, string outputArgName, int inputGateId, string inputArgName)
    {
        if (!gates.TryGetValue(outputGateId, out var outputGate))
        {
            throw new System.Exception($"Gate with ID {outputGateId} not found");
        }

        if (!gates.TryGetValue(inputGateId, out var inputGate))
        {
            throw new System.Exception($"Gate with ID {inputGateId} not found");
        }

        if (!outputGate.Outputs.TryGetValue(outputArgName, out var outputArgType))
        {
            throw new System.Exception($"The output gate does not have an output argument '{outputArgName}'");
        }

        if (!inputGate.Inputs.TryGetValue(inputArgName, out var inputArgType))
        {
            throw new System.Exception($"The input gate does not have an input argument '{inputArgName}'");
        }

        if (outputArgType != inputArgType)
        {
            throw new System.Exception($"Output '{outputArgName}' and input '{inputArgName}' are not of the same Scrapset type");
        }

        var link = new GateLink()
        {
            OutputGateId = outputGateId,
            OutputArgName = outputArgName,
            InputGateId = inputGateId,
            InputArgsName = inputArgName,
        };

        linksByOutputGate.Add(outputGateId, link);
        linksByInputGate.Add(inputGateId, link);
    }

    public void GoFromTo(int fromId, string flowName, int toId)
    {
        if (!gates.TryGetValue(fromId, out var fromGate))
        {
            throw new System.Exception($"Gate with ID {fromId} not found");
        }

        if (!gates.TryGetValue(toId, out var toGate))
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

        programFlows.Add(new ProgramFlow()
        {
            FromGate = fromStatement,
            FromFlowName = flowName,
            ToGate = toStatement,
        });
    }
}
