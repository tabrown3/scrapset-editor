using System.Collections.Generic;
using UnityEngine;

public class SubroutineInstance : MonoBehaviour
{
    private IStatement currentStatement;
    private IStatement nextStatement;

    // temporarily caches input values for all inputs of gates with dependencies
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedInputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    // temporarily caches output values for all outputs of evaluated gates
    Dictionary<int, Dictionary<string, ScrapsetValue>> cachedOutputValuesForGates = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
    // stores values that will persist during a single program execution before being wiped out
    Dictionary<string, ScrapsetValue> localVariableValues = new Dictionary<string, ScrapsetValue>();
    Dictionary<string, ScrapsetValue> inputVariableValues = new Dictionary<string, ScrapsetValue>();
    Dictionary<string, ScrapsetValue> outputVariableValues = new Dictionary<string, ScrapsetValue>();

    public SubroutineDefinition SubroutineDefinition { get; set; }

    public Dictionary<string, ScrapsetValue> Execute(Dictionary<string, ScrapsetValue> subroutineInputs)
    {
        Debug.Log("Program execution started!");
        InstantiateAllVariables(subroutineInputs);

        // entrypoint acts as the first statement to run; it does little more than Goto the real first statement
        var entrypoint = SubroutineDefinition.FindGateById(SubroutineDefinition.EntrypointId);
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
                throw new System.Exception("Statements must also implement IGate to be executed as part of the subroutine");
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

        Debug.Log("Program execution finished!");
        return outputVariableValues;
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
        foreach (var kv in SubroutineDefinition.GetInputLinks(callingGate.Id))
        {
            var inputParamName = kv.Key;
            var gateLink = kv.Value;
            var dependency = SubroutineDefinition.FindGateById(gateLink.OutputGateId);
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
                Debug.Log($"Used cached value of gate '{dependency.Name}' output '{gateLink.OutputParameterName}'");
            } else // otherwise the dependency needs to be evaluated and cached
            {
                Dictionary<string, ScrapsetValue> expressionOutputValues;
                var depIsAVariable = (dependency as IIdentifiable) != null; // variables do not have dependencies
                if (SubroutineDefinition.HasInputLinks(dependency.Id) && !depIsAVariable) // does it have dependencies that need evaluating?
                {
                    // Situation 1)
                    EvaluateDependencies(dependency); // update the global value store for all its dependencies
                    expressionOutputValues = dependencyAsExpression.Evaluate(cachedInputValuesForGates[dependency.Id]);
                    CacheOutputValuesForGate(dependency, expressionOutputValues);
                } else
                {
                    // if the dep's a variable, it needs the localVariablesValues to pull from - gates do not maintain their own state, even variables
                    //  otherwise just pass in an empty dict
                    var inputValues = depIsAVariable ? localVariableValues : new Dictionary<string, ScrapsetValue>();

                    // Situation 2)
                    expressionOutputValues = dependencyAsExpression.Evaluate(inputValues);
                    CacheOutputValuesForGate(dependency, expressionOutputValues);
                }

                var evaluatedValue = expressionOutputValues[gateLink.OutputParameterName];
                CacheInputValueForGate(callingGate, inputParamName, evaluatedValue);
            }
        }
    }

    private void InstantiateVariables(IReadOnlyDictionary<string, ScrapsetTypes> declarations, Dictionary<string, ScrapsetValue> variableStore)
    {
        foreach (var kv in declarations)
        {
            var variableName = kv.Key;
            var variableType = kv.Value;

            // basically allocating actual memory for the variables, one for each declared variable
            if (!variableStore.ContainsKey(variableName))
            {
                variableStore.Add(variableName, new ScrapsetValue(variableType));
            }

            // zero out local variables
            variableStore[variableName].Value = ScrapsetValue.GetDefaultForType(variableType);
        }
    }

    // the input variable store needs to be updated with whatever's pass into Execute
    private void InstantiateInputVariables(IReadOnlyDictionary<string, ScrapsetTypes> declarations, Dictionary<string, ScrapsetValue> variableStore, Dictionary<string, ScrapsetValue> subroutineInputs)
    {
        InstantiateVariables(declarations, variableStore);
        foreach (var kv in variableStore)
        {
            var variableName = kv.Key;

            // if the input wasn't linked to, it'll be blank - skip this iteration and just use the default zero'd value for variableName
            if (!subroutineInputs.ContainsKey(variableName))
            {
                continue;
            }

            if (!declarations.ContainsKey(variableName))
            {
                throw new System.Exception($"The variable '{variableName}' was pass to the subroutine but was never declared... how did that happen?");
            }

            // set input value store to the values passed to the subroutine
            variableStore[variableName].Value = subroutineInputs[variableName];
        }
    }

    private void InstantiateAllVariables(Dictionary<string, ScrapsetValue> subroutineInputs)
    {
        InstantiateVariables(SubroutineDefinition.LocalVariableDeclarations, localVariableValues);
        InstantiateInputVariables(SubroutineDefinition.InputParameters, inputVariableValues, subroutineInputs);
        InstantiateVariables(SubroutineDefinition.OutputParameters, outputVariableValues);
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

    // directly assign the value of inputName to outputName
    public void AssignInputToOutput<T>(T assigningGate, string inputName, string outputName) where T : IGate, IStatement
    {
        var gateLinks = SubroutineDefinition.GetOutputLinks(assigningGate.Id, outputName);

        foreach (var gateLink in gateLinks)
        {
            var variable = SubroutineDefinition.FindGateById(gateLink.InputGateId) as IWritable;
            if (variable == null)
            {
                throw new System.Exception($"Cannot assign to input '{gateLink.InputParameterName}' of Gate ID {gateLink.InputGateId}: Gate ID {gateLink.InputGateId} is not writable");
            }

            variable.Write(cachedInputValuesForGates[assigningGate.Id][inputName], localVariableValues);
        }

        Debug.Log($"Assigned gate '{assigningGate.Name}' input '{inputName}' with value {cachedInputValuesForGates[assigningGate.Id][inputName].Value} to output '{outputName}'");
    }

    // follow the program flow from the source statement to the statement it's linked to via the named flow link
    public void Goto<T>(T fromGate, string flowName) where T : IGate, IStatement
    {
        var programFlow = SubroutineDefinition.GetProgramFlowLink(fromGate, flowName);
        if (programFlow == null)
        {
            nextStatement = null;
            Debug.Log($"Followed outward path '{flowName}' from gate '{fromGate.Name}' with ID {fromGate.Id} to subroutine termination");
        } else
        {
            var toGateId = programFlow.ToGateId;
            var toGate = SubroutineDefinition.FindGateById(toGateId);
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
            Debug.Log($"Followed outward path '{flowName}' from gate '{fromGate.Name}' with ID {fromGate.Id} to gate '{toGate.Name}' with ID {toGate.Id}");
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
