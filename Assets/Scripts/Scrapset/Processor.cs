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
    // linksByGateIdInputParam is a Dictionary storing all the I/O links
    // outer key is IGate.Id, inner key is InputParameterName
    Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam = new Dictionary<int, Dictionary<string, GateLink>>();
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedInputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
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
        // entrypoint acts as the first statement to run; it does little more than Goto the real first statement
        var entrypoint = FindGateById(EntrypointId);
        currentStatement = entrypoint as IStatement;
        nextStatement = null;
        currentStatement.PerformSideEffect(this);

        // this is the main program loop going from statement to statement till it reaches the program end
        while (nextStatement != null) // basically "while there is a next statement"
        {
            currentStatement = nextStatement;

            var currentGate = currentStatement as IGate;
            if (currentGate == null)
            {
                throw new System.Exception("Statements must also implement IGate to be executed by the Processor");
            }

            EvaluateDependencies(currentGate);

            nextStatement = null;
            // the following PerformSideEffect will either Goto and set nextStatement to a new value or it won't,
            //  ending the program
            currentStatement.PerformSideEffect(this);
        }

        Debug.Log("Execution finished!");
    }

    private void EvaluateDependencies(IGate inGate)
    {
        if (linksByGateIdInputParam.TryGetValue(inGate.Id, out var linksByInputParam))
        {
            // this executes once for every output feeding into the gate's inputs
            foreach (var kv in linksByInputParam)
            {
                var inputParamName = kv.Key;
                var gateLink = kv.Value;
                var gate = FindGateById(gateLink.OutputGateId);
                Debug.Log($"Gate '{inGate.Name}' input param '{inputParamName}' is receiving from gate '{gate.Name}' output param '{gateLink.OutputParameterName}'");

                var expression = gate as IExpression;
                if (expression == null)
                {
                    throw new System.Exception($"Error with dependency feeding Gate {inGate.Name}: Gate {gate.Name} is not an expression");
                }

                // if it doesn't have values in the global value cache...
                if (!cachedInputValuesForGates.ContainsKey(gate.Id))
                {
                    Dictionary<string, ScrapsetValue> expressionOutputValues;
                    if (linksByGateIdInputParam.ContainsKey(gate.Id)) // does it have dependencies that need evaluating?
                    {
                        EvaluateDependencies(gate); // update the global value store for all its dependencies
                        expressionOutputValues = expression.Evaluate(cachedInputValuesForGates[gate.Id]);
                    } else // if not, just pass in an empty dict
                    {
                        expressionOutputValues = expression.Evaluate(new Dictionary<string, ScrapsetValue>());
                    }

                    var evaluatedValue = expressionOutputValues[gateLink.OutputParameterName];
                    CacheInputValueForGate(inGate, inputParamName, evaluatedValue);
                }
            }
        }
    }

    private void CacheInputValueForGate(IGate gate, string inputParamName, ScrapsetValue value)
    {
        if (!cachedInputValuesForGates.ContainsKey(gate.Id))
        {
            cachedInputValuesForGates.Add(gate.Id, new Dictionary<string, ScrapsetValue>());
        }

        cachedInputValuesForGates[gate.Id].Add(inputParamName, value);
    }

    public int SpawnGate<T>(string name) where T : IGate
    {
        var tempGameObj = new GameObject(name, typeof(T));
        var gate = tempGameObj.GetComponent<IGate>();
        gate.Id = idCounter++;
        gates.Add(gate.Id, gate);
        gateObjects.Add(gate.Id, tempGameObj);
        tempGameObj.transform.parent = transform;

        Debug.Log($"Spawning gate '{gate.Name}' with ID {gate.Id}");
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

        if (!linksByGateIdInputParam.ContainsKey(inputGateId))
        {
            linksByGateIdInputParam.Add(inputGateId, new Dictionary<string, GateLink>());
        }

        var linkByInput = linksByGateIdInputParam[inputGateId];
        if (!linkByInput.ContainsKey(inputParameterName))
        {
            linkByInput.Add(inputParameterName, link);
        } else
        {
            throw new System.Exception($"An I/O link entry for Gate ID {inputGateId} and input param '{inputParameterName}' already exists");
        }

        Debug.Log($"Linking gate '{outputGate.Name}' output '{outputParameterName}' to gate '{inputGate.Name}' input '{inputParameterName}'");
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

        Debug.Log($"Linking program flow from gate '{fromGate.Name}' to gate '{toGate.Name}'");
    }

    // directly assign the value of inputName to outputName
    public void AssignInputToOutput<T>(T assigningGate, string inputName, string outputName) where T : IGate, IStatement
    {
        Debug.Log($"Assigning gate '{assigningGate.Name}' input '{inputName}' with value {cachedInputValuesForGates[assigningGate.Id][inputName].Value} to output '{outputName}'");
    }

    // follow the program flow from the source statement to the statement it's linked to via the named flow link
    public void Goto<T>(T fromGate, string flowName) where T : IGate, IStatement
    {
        Debug.Log($"Following outward path '{flowName}' from gate '{fromGate.Name}'");

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
