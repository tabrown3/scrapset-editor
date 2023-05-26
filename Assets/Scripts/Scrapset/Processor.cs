using System.Collections.Generic;
using UnityEngine;

public class Processor : MonoBehaviour
{
    public int EntrypointId { get; private set; }

    private IStatement currentStatement;
    private IStatement nextStatement;

    int idCounter = 0;
    Dictionary<int, IGate> gates = new Dictionary<int, IGate>();
    Dictionary<int, GameObject> gateGameObjects = new Dictionary<int, GameObject>();
    // temporarily caches input values for all inputs of gates with dependencies
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedInputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    // temporarily caches output values for all outputs of evaluated gates
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedOutputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    // stores values that will persist during a single program execution before being wiped out
    Dictionary<string, ScrapsetValue> localVariableValues = new Dictionary<string, ScrapsetValue>();
    // a dictionary of local variable name -> gate ID, representing all gate instances of a local variable
    Dictionary<string, List<int>> localVariableInstances = new Dictionary<string, List<int>>();
    // manages the gate I/O connections
    GateIORegistry gateIORegistry;
    // manages the program flow connections
    ProgramFlowRegistry programFlowRegistry;

    public Processor()
    {
        gateIORegistry = new GateIORegistry(this);
        programFlowRegistry = new ProgramFlowRegistry(this);
    }

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
        // this executes once for every output feeding into the gate's inputs
        foreach (var kv in gateIORegistry.GetInputLinks(callingGate.Id))
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
                if (gateIORegistry.HasInputLinks(dependency.Id) && !depIsAVariable) // does it have dependencies that need evaluating?
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
        gateGameObjects.Add(gate.Id, tempGameObj);

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

        // destroy the game object and remove the gate reference from Processor
        Destroy(gateGameObjects[gateId]);
        gateGameObjects[gateId] = null;
        gates[gateId] = null;

        Debug.Log($"Removed gate '{name}' with ID {id}");
    }

    public IGate FindGateById(int id)
    {
        if (!gates.TryGetValue(id, out var gate))
        {
            return null;
        }

        return gate;
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
        if (localVariableValues.ContainsKey(variableName))
        {
            throw new System.Exception($"Variable '{variableName}' has already been declared in this scope");
        }

        localVariableValues.Add(variableName, new ScrapsetValue(scrapsetType));
    }

    public int SpawnVariable<T>(string variableName) where T : IGate, IVariable
    {
        if (!localVariableValues.TryGetValue(variableName, out var scrapsetValue))
        {
            throw new System.Exception($"Cannot spawn gate for variable '{variableName}': variable has not been declared");
        }

        var variableId = SpawnGate<T>(variableName);
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

        var gateLinks = gateIORegistry.GetOutputLinks(assigningGate.Id, outputName);

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

        var programFlow = programFlowRegistry.GetProgramFlowLink(fromGate, flowName);
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
