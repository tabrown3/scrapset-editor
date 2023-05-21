using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Processor : MonoBehaviour
{
    public int EntrypointId { get; private set; }
    private IStatement currentStatement;
    private IStatement nextStatement;

    int idCounter = 0;
    Dictionary<int, IGate> gates = new Dictionary<int, IGate>();
    Dictionary<int, GameObject> gateObjects = new Dictionary<int, GameObject>();
    Dictionary<int, Dictionary<string, GateLink>> linkByInputGate = new Dictionary<int, Dictionary<string, GateLink>>(); // outter key is IGate.Id, inner key is InputParam name, value is GateLink
    List<ProgramFlow> programFlows = new List<ProgramFlow>();

    int fakeCount = 0;

    void Start()
    {
        EntrypointId = SpawnGate<Entrypoint>("Entrypoint");
    }

    void Update()
    {
        if (programFlows.Count > 0 && fakeCount == 0)
        {
            RunProgram();
            fakeCount++; // TODO: DELETE - limited to a single run for testing purposes
        }
    }

    private void RunProgram()
    {
        var entrypoint = FindGateById(EntrypointId);
        currentStatement = entrypoint as IStatement;
        nextStatement = null;
        currentStatement.PerformSideEffect(this);

        while (nextStatement != null)
        {
            currentStatement = nextStatement;

            var currentGate = currentStatement as IGate;
            if (currentGate == null)
            {
                throw new System.Exception("Statements must also implement IGate to be executed by the Processor");
            }
            // find all gates outputting to currentGate

            nextStatement = null;
            currentStatement.PerformSideEffect(this);
        }

        Debug.Log("Execution finished!");
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

        var outputParameterType = outputGate.GetOutputParameter(outputParameterName);
        if (outputParameterType == ScrapsetTypes.None)
        {
            throw new System.Exception($"The output gate does not have an output parameter '{outputParameterName}'");
        }

        var inputParameterType = inputGate.GetInputParameter(inputParameterName);
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

        if (!linkByInputGate.ContainsKey(inputGateId))
        {
            linkByInputGate.Add(inputGateId, new Dictionary<string, GateLink>());
        }

        var linkByInput = linkByInputGate[inputGateId];
        if (!linkByInput.ContainsKey(inputParameterName))
        {
            linkByInput.Add(inputParameterName, link);
        } else
        {
            throw new System.Exception($"An I/O link entry for Gate ID {inputGateId} and input param '{inputParameterName}' already exists");
        }

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
            FromGateId = fromGate.Id,
            FromFlowName = flowName,
            ToGateId = toGate.Id,
        };

        programFlows.Add(programFlow);

        Debug.Log(programFlow);
    }

    // directly assign the value of inputName to outputName
    public void AssignInputToOutput<T>(T assigningGate, string inputName, string outputName) where T : IGate, IStatement
    {
        Debug.Log($"Assigning gate {assigningGate.Name} input {inputName} to target of {outputName}");
    }

    // follow the program flow from the source statement to the statement it's linked to via the named flow link
    public void Goto<T>(T fromGate, string flowName) where T : IGate, IStatement
    {
        Debug.Log($"Following {fromGate.Name} outward path '{flowName}'");

        var programFlow = programFlows.FirstOrDefault(u => u.FromGateId == fromGate.Id && u.FromFlowName == flowName);
        if (programFlow == null)
        {
            nextStatement = null;
        } else
        {
            var toGateId = programFlow.ToGateId;
            var toGate = FindGateById(toGateId);
            if (toGate == null)
            {
                throw new System.Exception($"Gate with ID {toGateId} not found");
            }

            var toStatement = toGate as IStatement;
            if (toStatement == null)
            {
                throw new System.Exception($"Gate with ID {toGateId} does not implement IStatement");
            }

            nextStatement = toStatement;
        }
    }
}
