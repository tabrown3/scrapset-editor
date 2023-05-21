using System.Collections.Generic;
using UnityEngine;

public class Processor : MonoBehaviour
{
    public int EntrypointId { get; private set; }

    int idCounter = 0;
    Dictionary<int, IGate> gates = new Dictionary<int, IGate>();
    Dictionary<int, GameObject> gateObjects = new Dictionary<int, GameObject>();
    Dictionary<int, GateLink> linksByOutputGate = new Dictionary<int, GateLink>();
    Dictionary<int, GateLink> linksByInputGate = new Dictionary<int, GateLink>();
    List<ProgramFlow> programFlows = new List<ProgramFlow>();

    // Start is called before the first frame update
    void Start()
    {
        EntrypointId = SpawnGate<Entrypoint>("Entrypoint");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int SpawnGate<T>(string name) where T : IGate
    {
        var tempGameObj = new GameObject(name, typeof(T));
        var gate = tempGameObj.GetComponent<IGate>();
        gate.Id = idCounter++;
        Debug.Log(gate.Id);
        gates.Add(gate.Id, gate);
        gateObjects.Add(gate.Id, tempGameObj);
        tempGameObj.transform.parent = transform;

        return gate.Id;
    }

    public IGate FindGateById(int id)
    {
        if (!gates.TryGetValue(id, out var gate))
        {
            return null;
        }

        return gate;
    }

    public ScrapsetTypes SelectGateInputParameter(IGate gate, string parameterName)
    {
        if (!gate.InputParameters.TryGetValue(parameterName, out var inputParameterType))
        {
            return ScrapsetTypes.None;
        }

        return inputParameterType;
    }

    public ScrapsetTypes SelectGateOutputParameter(IGate gate, string parameterName)
    {
        if (!gate.OutputParameters.TryGetValue(parameterName, out var outputParameterType))
        {
            return ScrapsetTypes.None;
        }

        return outputParameterType;
    }

    // create an I/O link between gates
    public void CreateInputOutputLink(int inputGateId, string inputParameterName, int outputGateId, string outputParameterName)
    {
        var outputGate = FindGateById(outputGateId);
        if (outputGate == null)
        {
            throw new System.Exception($"Could not find gate with ID ${outputGateId}");
        }

        var inputGate = FindGateById(inputGateId);
        if (inputGate == null)
        {
            throw new System.Exception($"Could not find gate with ID ${inputGateId}");
        }

        var outputParameterType = SelectGateOutputParameter(outputGate, outputParameterName);
        if (outputParameterType == ScrapsetTypes.None)
        {
            throw new System.Exception($"The output gate does not have an output parameter '{outputParameterName}'");
        }

        var inputParameterType = SelectGateInputParameter(inputGate, inputParameterName);
        if (inputParameterType == ScrapsetTypes.None)
        {
            throw new System.Exception($"The input gate does not have an input parameter '{inputParameterName}'");
        }

        // TODO: in the future, perform logic with Generics here for more dynamic type checking
        if (outputParameterType != inputParameterType)
        {
            throw new System.Exception($"Output '{outputParameterName}' and input '{inputParameterName}' are not of the same Scrapset type");
        }

        var link = new GateLink()
        {
            OutputGateId = outputGateId,
            OutputParameterName = outputParameterName,
            InputGateId = inputGateId,
            InputParameterName = inputParameterName,
        };

        linksByOutputGate.Add(outputGateId, link);
        linksByInputGate.Add(inputGateId, link);

        Debug.Log(link);
    }

    // establish program execution order by linking statements together
    public void CreateProgramFlowLink(int fromId, string flowName, int toId)
    {
        var fromGate = FindGateById(fromId);
        if (fromGate == null)
        {
            throw new System.Exception($"Gate with ID {fromId} not found");
        }

        var toGate = FindGateById(toId);
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
            FromGate = fromStatement,
            FromFlowName = flowName,
            ToGate = toStatement,
        };

        programFlows.Add(programFlow);

        Debug.Log(programFlow);
    }

    // directly assign the value of inputName to outputName
    public void AssignInputToOutput<T>(T assigningGate, string inputName, string outputName) where T : IGate, IStatement
    {

    }

    // follow the program flow from the source statement to the statement it's linked to via the named flow link
    public void Goto<T>(T sourceGate, string flowName) where T : IGate, IStatement
    {

    }
}
