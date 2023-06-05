using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapset.Engine
{
    public class SubroutineInstance
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
            var entrypoint = SubroutineDefinition.GetGateById(SubroutineDefinition.EntrypointId);
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
        private Dictionary<int, Dictionary<string, ScrapsetValue>> EvaluateDependencies(IGate callingGate)
        {
            var allDeps = new Dictionary<int, Dictionary<string, ScrapsetValue>>();
            // this executes once for every output feeding into the gate's inputs
            foreach (var kv in SubroutineDefinition.GetInputLinks(callingGate.Id))
            {
                if (callingGate.InputParameters[kv.Key].IsDeferred) continue;
                allDeps[callingGate.Id] = EvaluateDependency(callingGate, kv.Key, kv.Value);
            }

            return allDeps;
        }

        // creates a kv pair from output param names -> dep eval callbacks
        private Dictionary<string, LazyEvaluateDependency> LazyEvaluateDependencies(IGate callingGate)
        {
            var evalDepCallbacksByInputName = new Dictionary<string, LazyEvaluateDependency>();
            // this executes once for every output feeding into the gate's inputs
            foreach (var kv in SubroutineDefinition.GetInputLinks(callingGate.Id))
            {
                if (!callingGate.InputParameters[kv.Key].IsDeferred) continue;
                // evaluates the dep when cb is called and selects out the desired dep output, assigning it to the desired input in the out dict
                evalDepCallbacksByInputName[kv.Key] = () => EvaluateDependency(callingGate, kv.Key, kv.Value)[kv.Value.OutputParameterName];
            }

            return evalDepCallbacksByInputName;
        }

        public delegate ScrapsetValue LazyEvaluateDependency();

        private Dictionary<string, ScrapsetValue> EvaluateDependency(IGate callingGate, string inputParamName, GateLink gateLink)
        {
            var dependency = SubroutineDefinition.GetGateById(gateLink.OutputGateId);
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
                var cachedExpressionOutputValues = cachedOutputValuesForGates[dependency.Id];
                var evaluatedValue = cachedExpressionOutputValues[gateLink.OutputParameterName];
                CacheInputValueForGate(callingGate, inputParamName, evaluatedValue);
                Debug.Log($"Used cached value of gate '{dependency.Name}' output '{gateLink.OutputParameterName}'");
                return cachedExpressionOutputValues;
            } else // otherwise the dependency needs to be evaluated and cached
            {
                Dictionary<string, ScrapsetValue> expressionOutputValues;
                var identifiableDep = dependency as IIdentifiable; // variables do not have dependencies
                if (SubroutineDefinition.HasInputLinks(dependency.Id) && identifiableDep == null) // does it have dependencies that need evaluating?
                {
                    // the input params are iterated over and, depending on whether the dependency eval
                    //  IsDeferred or not, the value for the param will either come from LazyEvaluateDependencies
                    //  or EvaluateDependencies, but not both
                    // Situation 1) - deferred
                    var lazyEvalCallbacks = LazyEvaluateDependencies(dependency);
                    // Situation 1)
                    EvaluateDependencies(dependency); // update the global value store for all its dependencies

                    var nullCheckedCachedInputValuesForDep = new Dictionary<string, ScrapsetValue>();
                    if (cachedInputValuesForGates.TryGetValue(dependency.Id, out Dictionary<string, ScrapsetValue> cachedValuesForDep))
                    {
                        nullCheckedCachedInputValuesForDep = cachedValuesForDep;
                    }

                    expressionOutputValues = dependencyAsExpression.Evaluate(nullCheckedCachedInputValuesForDep, lazyEvalCallbacks);
                    CacheOutputValuesForGate(dependency, expressionOutputValues);
                } else
                {
                    if (identifiableDep == null)
                    {
                        // if the gate isn't identifiable, then it's just a gate that doesn't have dependencies, so pass in an empty dict
                        expressionOutputValues = dependencyAsExpression.Evaluate(new Dictionary<string, ScrapsetValue>(), new Dictionary<string, LazyEvaluateDependency>());
                        CacheOutputValuesForGate(dependency, expressionOutputValues);
                    } else // it's some kind of variable or sr input
                    {
                        var isLocalVariable = SubroutineDefinition.LocalVariableDeclarations.ContainsKey(identifiableDep.Identifier);
                        var isSubroutineInput = SubroutineDefinition.InputParameters.ContainsKey(identifiableDep.Identifier);

                        if (!isLocalVariable && !isSubroutineInput)
                        {
                            throw new System.Exception($"Variable '{identifiableDep.Identifier}' is not declared as a local variable or a subroutine input");
                        }

                        // it's a regular old variable
                        if (isLocalVariable)
                        {
                            expressionOutputValues = dependencyAsExpression.Evaluate(localVariableValues, new Dictionary<string, LazyEvaluateDependency>());
                            CacheOutputValuesForGate(dependency, expressionOutputValues);
                        } else if (isSubroutineInput)
                        {
                            // it's a subroutine input gate
                            expressionOutputValues = dependencyAsExpression.Evaluate(inputVariableValues, new Dictionary<string, LazyEvaluateDependency>());
                            CacheOutputValuesForGate(dependency, expressionOutputValues);
                        } else
                        {
                            throw new System.Exception($"The identifier {identifiableDep.Identifier} was declared as a local variable and a subroutine input: cannot be both");
                        }
                    }
                }

                var evaluatedValue = expressionOutputValues[gateLink.OutputParameterName];
                CacheInputValueForGate(callingGate, inputParamName, evaluatedValue);
                return expressionOutputValues;
            }
        }

        private void InstantiateVariables(List<Tuple<string, ScrapsetTypes>> declarations, Dictionary<string, ScrapsetValue> variableStore)
        {
            foreach (var (variableName, variableType) in declarations)
            {
                // basically allocating actual memory for the variables, one for each declared variable
                if (!variableStore.ContainsKey(variableName))
                {
                    variableStore.Add(variableName, new ScrapsetValue(variableType));
                }

                // zero-out local variables
                variableStore[variableName].Value = ScrapsetValue.GetDefaultForType(variableType);
            }
        }

        // the input variable store needs to be updated with whatever's pass into Execute
        private void InstantiateInputVariables(List<Tuple<string, ScrapsetTypes>> declarations, Dictionary<string, ScrapsetValue> variableStore, Dictionary<string, ScrapsetValue> subroutineInputs)
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

                if (!declarations.Exists(u => u.Item1 == variableName))
                {
                    throw new System.Exception($"The variable '{variableName}' was passed to the subroutine but was never declared... how did that happen?");
                }

                // set input value store to the values passed to the subroutine
                variableStore[variableName].Value = subroutineInputs[variableName].Value;
            }
        }

        private void InstantiateAllVariables(Dictionary<string, ScrapsetValue> subroutineInputs)
        {
            var localVars = SubroutineDefinition.LocalVariableDeclarations.Select(u => Tuple.Create(u.Key, u.Value)).ToList();
            InstantiateVariables(localVars, localVariableValues);
            var inputParams = SubroutineDefinition.InputParameters.Select(u => Tuple.Create(u.Key, u.Value.Type)).ToList();
            InstantiateInputVariables(inputParams, inputVariableValues, subroutineInputs);
            var outputParams = SubroutineDefinition.OutputParameters.Select(u => Tuple.Create(u.Key, u.Value.Type)).ToList();
            InstantiateVariables(outputParams, outputVariableValues);
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
                var variable = SubroutineDefinition.GetGateById(gateLink.InputGateId) as IWritable;
                var identifable = variable as IIdentifiable;
                if (variable == null)
                {
                    throw new System.Exception($"Cannot assign to input '{gateLink.InputParameterName}' of Gate ID {gateLink.InputGateId}: Gate ID {gateLink.InputGateId} is not writable");
                }

                // are we assigning to a local variable or a subroutine output gate?
                Dictionary<string, ScrapsetValue> variableStore;
                if (SubroutineDefinition.OutputParameters.ContainsKey(identifable.Identifier))
                {
                    variableStore = outputVariableValues;
                } else
                {
                    variableStore = localVariableValues;
                }

                variable.Write(cachedInputValuesForGates[assigningGate.Id][inputName], variableStore);
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
                var toGate = SubroutineDefinition.GetGateById(toGateId);
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
}
