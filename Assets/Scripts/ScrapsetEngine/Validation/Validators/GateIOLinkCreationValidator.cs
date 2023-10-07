using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class GateIOLinkCreationValidator : IScrapsetValidator
    {
        Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam;
        SubroutineDefinition subroutineDefinition;
        int inputGateId;
        string inputParameterName;
        int outputGateId;
        string outputParameterName;

        public GateIOLinkCreationValidator(
            Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam,
            SubroutineDefinition subroutineDefinition,
            int inputGateId,
            string inputParameterName,
            int outputGateId,
            string outputParameterName)
        {
            this.linksByGateIdInputParam = linksByGateIdInputParam;
            this.subroutineDefinition = subroutineDefinition;
            this.inputGateId = inputGateId;
            this.inputParameterName = inputParameterName;
            this.outputGateId = outputGateId;
            this.outputParameterName = outputParameterName;
        }

        public ValidationResult Validate()
        {
            var outputGate = subroutineDefinition.GetGateById(outputGateId);
            if (outputGate == null)
            {
                throw new System.Exception($"Could not find gate with ID ${outputGateId}");
            }

            var inputGate = subroutineDefinition.GetGateById(inputGateId);
            if (inputGate == null)
            {
                throw new System.Exception($"Could not find gate with ID ${inputGateId}");
            }

            var outputParameter = outputGate.GetOutputParameter(outputParameterName);
            if (outputParameter.Type == ScrapsetTypes.None)
            {
                throw new System.Exception($"The output gate '{outputGate.GetType()}' with ID {outputGateId} does not have an output parameter '{outputParameterName}'");
            }

            var inputParameter = inputGate.GetInputParameter(inputParameterName);
            if (inputParameter.Type == ScrapsetTypes.None)
            {
                throw new System.Exception($"The input gate '{inputGate.GetType()}' with ID {inputGateId} does not have an input parameter '{inputParameterName}'");
            }

            /*** Beginning of generic type checking/inference logic ***/
            var inferredInputParam = inputParameter; // it's the inferred types that actually undergo the type equivalency check at the end
            var inferredOutputParam = outputParameter;
            if (inputParameter.Type == ScrapsetTypes.Generic || outputParameter.Type == ScrapsetTypes.Generic)
            {
                // this scenario (connecting up two generic ports) is not currently supported, but likely will be in the future
                //  -- consider storing a dictionary of "deferred judgements," where the GateIORegistry checks back when one of
                //  --  the generics for the gates is inferred in the future and then sets the other
                if (inputParameter.Type == ScrapsetTypes.Generic && outputParameter.Type == ScrapsetTypes.Generic)
                {
                    throw new System.Exception("Connecting two generic ports is not supported at this time, sorry!");
                }

                // if the input param's the generic
                if (inputParameter.Type == ScrapsetTypes.Generic)
                {
                    var genericIdentifier = inputGate.GenericTypeReconciler.GetGenericIdentifierOfInputParam(inputParameterName);
                    if (genericIdentifier == null)
                    {
                        throw new System.Exception($"Cannot determine generic type: input param '{inputParameterName}' of gate '{inputGate.GetType()}' is set as generic but has no generic identifier (e.g. 'T')");
                    }

                    // you pass in "T" and it returns the currently inferred type for "T"
                    var genericType = inputGate.GenericTypeReconciler.GetTypeOfGenericIdentifier(genericIdentifier);
                    if (genericType == ScrapsetTypes.None)
                    {
                        // if "T" doesn't have an inferred type set, set one based on the other param's type
                        inferredInputParam.Type = outputParameter.Type;
                    } else
                    {
                        // otherwise use the existing inferred type
                        inferredInputParam.Type = genericType;
                    }

                } else // if the output param's the generic
                {
                    var genericIdentifier = outputGate.GenericTypeReconciler.GetGenericIdentifierOfOutputParam(outputParameterName);
                    if (genericIdentifier == null)
                    {
                        throw new System.Exception($"Cannot determine generic type: output param '{outputParameterName}' of gate '{outputGate.GetType()}' is set as generic but has no generic identifier (e.g. 'T')");
                    }

                    var genericType = outputGate.GenericTypeReconciler.GetTypeOfGenericIdentifier(genericIdentifier);
                    if (genericType == ScrapsetTypes.None)
                    {
                        inferredOutputParam.Type = inputParameter.Type;
                    } else
                    {
                        inferredOutputParam.Type = genericType;
                    }
                }
            }

            if (inferredInputParam.Type != inferredOutputParam.Type)
            {
                throw new System.Exception($"Output '{outputParameterName}' ({inferredOutputParam}) and input '{inputParameterName}' ({inferredInputParam}) are not of the same Scrapset type");
            }

            Dictionary<string, GateLink> outLinksByInputParam;
            if (linksByGateIdInputParam.TryGetValue(inputGateId, out outLinksByInputParam)
                && outLinksByInputParam.ContainsKey(inputParameterName))
            {
                // The rationale here is that an output can serve as a data source for any number of inputs, but an input can only accept data from a
                //  single source.
                var existingLink = linksByGateIdInputParam[inputGateId][inputParameterName];
                throw new System.Exception($"Input param '{inputParameterName}' for calling gate ID {inputGateId} is" +
                    $"already linked to output param '{existingLink.OutputParameterName}' of source gate ID {existingLink.OutputGateId}");
            }

            throw new System.NotImplementedException();
        }
    }
}
