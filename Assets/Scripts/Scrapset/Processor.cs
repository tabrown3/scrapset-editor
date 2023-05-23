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
    // linksByGateIdInputParam is a deep Dictionary storing all the I/O links by the calling gate's ID and input param name.
    //  Outer key is IGate.Id, inner key is InputParameterName
    Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam = new Dictionary<int, Dictionary<string, GateLink>>();
    // linksByGateIdOutputParam is a deep Dictionary storing all the I/O links by the source (outputting) gate's ID and output param name.
    //  Outer key is IGate.Id, inner key is InputParameterName, value is a list of all GateLinks for which this output is a data source
    Dictionary<int, Dictionary<string, List<GateLink>>> linksByGateIdOutputParam = new Dictionary<int, Dictionary<string, List<GateLink>>>();
    // temporarily caches input values for all inputs of gates with dependencies
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedInputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    // temporarily caches output values for all outputs of evaluated gates
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedOutputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    List<ProgramFlow> programFlows = new List<ProgramFlow>();
    // stores values that will persist during a single program execution before being wiped out
    Dictionary<string, ScrapsetValue> localVariableValues = new Dictionary<string, ScrapsetValue>();
    // a dictionary of local variable name -> gate ID, representing all gates instances of a local variable
    Dictionary<string, List<int>> localVariableInstances = new Dictionary<string, List<int>>();

    void Start()
    {
        EntrypointId = SpawnGate<Entrypoint>("Entrypoint");
    }

    void Update()
    {

    }

    public void RunProgram()
    {
        Debug.Log("Program execution started!");

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

            Debug.Log($"Started statement execution for gate '{currentGate.Name}' with ID {currentGate.Id}");

            // kick off the recursive dependency evaluation; all of the statement's deps will be eval'd
            //  depth-first and the results latched on both the dep's output param and the caller's
            //  input param
            EvaluateDependencies(currentGate);

            nextStatement = null;
            // the following PerformSideEffect will either Goto and set nextStatement to a new value or it won't,
            //  ending the program
            currentStatement.PerformSideEffect(this);

            cachedInputValuesForGates.Clear(); // clear the input value cache after each statement finishes
            cachedOutputValuesForGates.Clear(); // clear the output value cache after each statement finishes
            Debug.Log($"Finished statement execution for gate '{currentGate.Name}' with ID {currentGate.Id}");
        }

        // when program completes, resets all variables to the zero value
        foreach (var variable in localVariableValues.Values)
        {
            variable.Value = ScrapsetValue.GetDefaultForType(variable.Type);
        }

        Debug.Log("Program execution finished!");
    }

    // EvaluateDependencies will recurse over the calling gate's expression dependency tree depth-first.
    //  It attempts to evaluate each expression it encounters. There are 3 situations it might face:
    //  1) The expression itself has dependencies - it will move to evaluate those dependencies first
    //     recursively and, once the deps are available, evaluate the expression and cache the results
    //  2) The expression has no dependencies of its own, or is a variable - it will evaluate the expression
    //     and cache the results
    //  3) The expression has already been evaluated and cached - it will not re-evaluate but instead
    //     use the cached value
    private void EvaluateDependencies(IGate callingGate)
    {
        if (linksByGateIdInputParam.TryGetValue(callingGate.Id, out var linksByInputParam))
        {
            // this executes once for every output feeding into the gate's inputs
            foreach (var kv in linksByInputParam)
            {
                var inputParamName = kv.Key;
                var gateLink = kv.Value;
                var dependency = FindGateById(gateLink.OutputGateId);
                Debug.Log($"Gate '{callingGate.Name}' input param '{inputParamName}' is receiving from gate '{dependency.Name}' output param '{gateLink.OutputParameterName}'");

                var dependencyAsExpression = dependency as IExpression;
                if (dependencyAsExpression == null)
                {
                    throw new System.Exception($"Error with dependency feeding Gate {callingGate.Name}: Gate {dependency.Name} is not an expression");
                }

                // check to see if this dependency has already been evaluated and had its outputs cached
                if (DependencyHasBeenEvaluated(dependency))
                {
                    // Situation 3)
                    // use the cached values instead of re-evaluating the dependency
                    var evaluatedValue = cachedOutputValuesForGates[dependency.Id][gateLink.OutputParameterName];
                    CacheInputValueForGate(callingGate, inputParamName, evaluatedValue);
                    Debug.Log($"Using cached value of gate '{dependency.Name}' output '{gateLink.OutputParameterName}'");
                } else // otherwise the dependency needs to be evaluated and cached
                {
                    Dictionary<string, ScrapsetValue> expressionOutputValues;
                    var depIsAVariable = (dependency as IVariable) != null; // variables do not have dependencies
                    if (linksByGateIdInputParam.ContainsKey(dependency.Id) && !depIsAVariable) // does it have dependencies that need evaluating?
                    {
                        // Situation 1)
                        EvaluateDependencies(dependency); // update the global value store for all its dependencies
                        expressionOutputValues = dependencyAsExpression.Evaluate(cachedInputValuesForGates[dependency.Id]);
                        CacheOutputValuesForGate(dependency, expressionOutputValues);
                    } else // if not, just pass in an empty dict
                    {
                        // Situation 2)
                        expressionOutputValues = dependencyAsExpression.Evaluate(new Dictionary<string, ScrapsetValue>());
                        CacheOutputValuesForGate(dependency, expressionOutputValues);
                    }

                    var evaluatedValue = expressionOutputValues[gateLink.OutputParameterName];
                    CacheInputValueForGate(callingGate, inputParamName, evaluatedValue);
                }
            }
        }
    }

    // after a gate's dependency has been evaluated and its output values are available, this method
    //  is called to cache that output value within a Dictionary of Dictionaries, where the outer key
    //  is the caller's Gate.Id and the inner key is the caller's input param name
    private void CacheInputValueForGate(IGate gate, string inputParamName, ScrapsetValue value)
    {
        if (!cachedInputValuesForGates.ContainsKey(gate.Id))
        {
            cachedInputValuesForGates.Add(gate.Id, new Dictionary<string, ScrapsetValue>());
        }

        cachedInputValuesForGates[gate.Id].Add(inputParamName, value);
    }

    // after a gate has been evaluated and its output values are available, this method is called
    //  to cache all of the returned output values with respect to their Gate.Id and output param name
    // Note: the difference between this and the Input varaint is this latches the value on the output
    // gate's output parameter; the input variant latches the value on the calling gate's input parameter
    private void CacheOutputValuesForGate(IGate gate, Dictionary<string, ScrapsetValue> values)
    {
        if (!cachedOutputValuesForGates.ContainsKey(gate.Id))
        {
            cachedOutputValuesForGates.Add(gate.Id, values);
        }
    }

    // if the output cache contains an entry for the gate, that means it had been eval'd earlier in the
    //  current statement's execution and had its outputs cached
    private bool DependencyHasBeenEvaluated(IGate gate)
    {
        return cachedOutputValuesForGates.ContainsKey(gate.Id);
    }

    public int SpawnGate<T>(string name) where T : IGate
    {
        var tempGameObj = new GameObject(name, typeof(T));
        var gate = tempGameObj.GetComponent<IGate>();
        gate.Id = idCounter++;
        gates.Add(gate.Id, gate);
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
            // The rationale here is that an output can serve as a data source for any number of inputs, but an input can only accept data from a
            //  single source.
            var existingLink = linksByGateIdInputParam[inputGateId][inputParameterName];
            throw new System.Exception($"Input param '{inputParameterName}' for calling gate ID {inputGateId} is" +
                $"already linked to output param '{existingLink.OutputParameterName}' of source gate ID {existingLink.OutputGateId}");
        }

        if (!linksByGateIdOutputParam.ContainsKey(outputGateId))
        {
            linksByGateIdOutputParam.Add(outputGateId, new Dictionary<string, List<GateLink>>());
        }

        var linkListByOutput = linksByGateIdOutputParam[outputGateId];
        if (!linkListByOutput.ContainsKey(outputParameterName))
        {
            linkListByOutput.Add(outputParameterName, new List<GateLink>());
        }

        var linkList = linkListByOutput[outputParameterName];
        if (linkList.Any(u => u.InputParameterName == inputParameterName))
        {
            // The rationale here is that an output can serve as a data source for any number of inputs, but an input can only accept data from a
            //  single source. We determined in the check above that the input doesn't have a source, so this could only result as a bug.
            throw new System.Exception($"The output list for gate ID {outputGateId} already contains an entry for input param" +
                $"'{inputParameterName}' of gate ID {inputGateId}. This is likely a bug in the Processor.");
        } else
        {
            linkList.Add(link);
        }

        Debug.Log($"Linking gate '{outputGate.Name}' output '{outputParameterName}' to gate '{inputGate.Name}' input '{inputParameterName}'");
    }

    public void RemoveInputOutputLink(int inputGateId, string inputParameterName)
    {
        if (!linksByGateIdInputParam.TryGetValue(inputGateId, out var linksByInputParam))
        {
            throw new System.Exception($"Cannot remove I/O link: Gate ID {inputGateId} has no linked inputs");
        }

        if (!linksByInputParam.ContainsKey(inputParameterName))
        {
            throw new System.Exception($"Cannot remove I/O link: input '{inputParameterName}' of Gate ID {inputGateId} is not linked");
        }

        linksByInputParam.Remove(inputParameterName, out var gateLink);

        var outputGateId = gateLink.OutputGateId;
        var outputParameterName = gateLink.OutputParameterName;

        if (!linksByGateIdOutputParam.TryGetValue(outputGateId, out var linksByOutputParam))
        {
            throw new System.Exception($"Cannot remove I/O link: Gate ID {outputGateId} has no linked outputs");
        }

        if (!linksByOutputParam.ContainsKey(outputParameterName))
        {
            throw new System.Exception($"Cannot remove I/O link: output '{outputParameterName}' of Gate ID {outputGateId} is not linked");
        }

        linksByOutputParam[outputParameterName].Remove(gateLink);

        Debug.Log($"Deleting link for gate ID {outputGateId} output '{outputParameterName}' and gate ID {inputGateId} input '{inputParameterName}'");
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

    public void RemoveProgramFlowLink(int fromId, string flowName)
    {
        var programFlow = programFlows.Find(u => u.FromGateId == fromId && u.FromFlowName == flowName);
        if (programFlow == null)
        {
            throw new System.Exception($"Cannot remove program flow: no program flow links from path '{flowName}' of gate ID {fromId}");
        }

        programFlows.Remove(programFlow);

        Debug.Log($"Deleting program flow from gate ID {fromId} path '{flowName}'");
    }

    public void DeclareLocalVariable(string variableName, ScrapsetTypes scrapsetType)
    {
        if (localVariableValues.ContainsKey(variableName))
        {
            throw new System.Exception($"Variable '{variableName}' has already been declared in this scope");
        }

        localVariableValues.Add(variableName, new ScrapsetValue(scrapsetType));
    }

    public int SpawnVariable(string variableName)
    {
        if (!localVariableValues.TryGetValue(variableName, out var scrapsetValue))
        {
            throw new System.Exception($"Cannot spawn gate for variable '{variableName}': variable has not been declared");
        }

        var variableId = SpawnGate<NumberVariableGate>(variableName);
        var newVariable = FindGateById(variableId) as IVariable;
        newVariable.Reference = localVariableValues[variableName];
        newVariable.VariableName = variableName;

        if (!localVariableInstances.ContainsKey(variableName))
        {
            localVariableInstances.Add(variableName, new List<int>());
        }

        localVariableInstances[variableName].Add(variableId);
        return variableId;
    }

    // directly assign the value of inputName to outputName
    public void AssignInputToOutput<T>(T assigningGate, string inputName, string outputName) where T : IGate, IStatement
    {
        Debug.Log($"Assigning gate '{assigningGate.Name}' input '{inputName}' with value {cachedInputValuesForGates[assigningGate.Id][inputName].Value} to output '{outputName}'");

        if (!linksByGateIdOutputParam.TryGetValue(assigningGate.Id, out var linksByOutputParam))
        {
            throw new System.Exception($"Assigning gate '{assigningGate.Name}' with ID {assigningGate.Id} does not have any outbound links");
        }

        if (!linksByOutputParam.TryGetValue(outputName, out var gateLinks))
        {
            throw new System.Exception($"Assigning gate '{assigningGate.Name}' with ID {assigningGate.Id} output {outputName} is not linked to any inputs");
        }

        foreach (var gateLink in gateLinks)
        {
            var variable = FindGateById(gateLink.InputGateId) as IVariable;
            if (variable == null)
            {
                throw new System.Exception($"Cannot assign to input '{gateLink.InputParameterName}' of Gate ID {gateLink.InputGateId}: Gate ID {gateLink.InputGateId} is not a variable");
            }

            variable.Write(cachedInputValuesForGates[assigningGate.Id][inputName]);
        }
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

    public ScrapsetValue GetCachedInputValue(int id, string inputName)
    {
        if (!cachedInputValuesForGates.TryGetValue(id, out var cachedInputValues))
        {
            throw new System.Exception($"Gate ID {id} doesn't contain any cached input values for the current scope");
        }

        if (!cachedInputValues.TryGetValue(inputName, out var cachedValue))
        {
            throw new System.Exception($"Gate ID {id} doesn't contain a cached value for input {inputName}");
        }

        return cachedValue;
    }

    public ScrapsetValue GetCachedOutputValue(int id, string outputName)
    {
        if (!cachedOutputValuesForGates.TryGetValue(id, out var cachedOutputValues))
        {
            throw new System.Exception($"Gate ID {id} doesn't contain any cached output values for the current scope");
        }

        if (!cachedOutputValues.TryGetValue(outputName, out var cachedValue))
        {
            throw new System.Exception($"Gate ID {id} doesn't contain a cached value for output {outputName}");
        }

        return cachedValue;
    }
}
