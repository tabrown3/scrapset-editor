using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapset.Engine
{
    public class GateIORegistry
    {
        // linksByGateIdInputParam is a deep Dictionary storing all the I/O links by the calling gate's ID and input param name.
        //  Outer key is IGate.Id, inner key is InputParameterName
        Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam = new Dictionary<int, Dictionary<string, GateLink>>();
        // linksByGateIdOutputParam is a deep Dictionary storing all the I/O links by the source (outputting) gate's ID and output param name.
        //  Outer key is IGate.Id, inner key is InputParameterName, value is a list of all GateLinks for which this output is a data source
        Dictionary<int, Dictionary<string, List<GateLink>>> linksByGateIdOutputParam = new Dictionary<int, Dictionary<string, List<GateLink>>>();

        SubroutineDefinition subroutineDefinition;

        public GateIORegistry(SubroutineDefinition _subroutineDefinition)
        {
            subroutineDefinition = _subroutineDefinition;
        }

        public bool TryGetInputLinks(int gateId, out Dictionary<string, GateLink> linksByInputParam)
        {
            return linksByGateIdInputParam.TryGetValue(gateId, out linksByInputParam);
        }

        public bool HasInputLinks(int gateId)
        {
            if (!TryGetInputLinks(gateId, out var linksByInputParam))
            {
                return false;
            }

            return linksByInputParam != null && linksByInputParam.Count > 0;
        }

        public IDictionary<string, GateLink> GetInputLinks(int gateId)
        {
            if (TryGetInputLinks(gateId, out var linksByInputParam))
            {
                return linksByInputParam;
            }

            return new Dictionary<string, GateLink>();
        }

        public bool TryGetInputLink(int gateId, string parameterName, out GateLink gateLink)
        {
            if (!TryGetInputLinks(gateId, out var linksByInputParam))
            {
                gateLink = null;
                return false;
            }

            return linksByInputParam.TryGetValue(parameterName, out gateLink);
        }

        public bool HasInputLink(int gateId, string parameterName)
        {
            return TryGetInputLink(gateId, parameterName, out _);
        }

        public List<GateLink> GetOutputLinks(int gateId, string parameterName)
        {
            if (!linksByGateIdOutputParam.TryGetValue(gateId, out var linksByOutputParam))
            {
                return new List<GateLink>();
            }

            if (!linksByOutputParam.TryGetValue(parameterName, out var gateLinks))
            {
                return new List<GateLink>();
            }

            return new List<GateLink>(gateLinks);
        }

        // create an I/O link between gates
        public void CreateInputOutputLink(int inputGateId, string inputParameterName, int outputGateId, string outputParameterName)
        {
            /*************************************************
            *** START VALIDATION - DO NOT MODIFY STATE HERE ****
            *************************************************/

            /*** NOTE on validations ***/
            /* The following validations should be the counterparts of the state manipulations that take place
             *  later on. DO NOT MODIFY STATE HERE. If state is modified here but an error is detected afterward,
             *  the result is partially updated state that's ultimately in an invalid state. We want to avoid that
             *  at all cost, even if it means duplicating a few if/else blocks in the state manipulation section.
             */

            var result = GateIOLinkRuleset.ValidateLinkCreation(subroutineDefinition, inputGateId, inputParameterName, outputGateId, outputParameterName);

            if (result.HasErrors)
            {
                var error = result.Errors.First();
                throw new System.Exception(error.ToString());
            }

            var inputParameter = result.ComputedValues.InputParameter;
            var outputParameter = result.ComputedValues.OutputParameter;
            var inputGate = result.ComputedValues.InputGate;
            var outputGate = result.ComputedValues.OutputGate;

            /*************************************************
            *** END VALIDATION - START STATE MANIPULATION ****
            *************************************************/

            /*** NOTE on state manipulations ***/
            /* The following state manipulations should be the counterparts of validation check that have already
             *  taken place. What that means is no validation takes place below, only state changes. If you need
             *  to perform validation, do it in the validation section.
             */

            if (inputParameter.Type == ScrapsetTypes.Generic || outputParameter.Type == ScrapsetTypes.Generic)
            {
                if (inputParameter.Type == ScrapsetTypes.Generic)
                {
                    var genericIdentifier = inputGate.GenericTypeReconciler.GetGenericIdentifierOfInputParam(inputParameterName);
                    var genericType = inputGate.GenericTypeReconciler.GetTypeOfGenericIdentifier(genericIdentifier);
                    if (genericType == ScrapsetTypes.None)
                    {
                        // if "T" doesn't have an inferred type set, set one based on the other param's type
                        inputGate.GenericTypeReconciler.SetInputGenericTypeMapping(inputParameterName, outputParameter.Type);
                        Debug.Log($"Input param '{inputParameterName}' of gate '{inputGate.GetType()}' with ID {inputGateId} had its generic identifier '{genericIdentifier}' set to {outputParameter} from the output param '{outputParameterName}' of gate '{outputGate.GetType()}' with ID {outputGateId}");
                    } else
                    {
                        Debug.Log($"Input param '{inputParameterName}' of gate '{inputGate.GetType()}' with ID {inputGateId} had its generic identifier '{genericIdentifier}' inferred as {genericType} from a previous connection involving generic identifier '{genericIdentifier}'");
                    }
                } else
                {
                    var genericIdentifier = outputGate.GenericTypeReconciler.GetGenericIdentifierOfOutputParam(outputParameterName);
                    var genericType = outputGate.GenericTypeReconciler.GetTypeOfGenericIdentifier(genericIdentifier);
                    if (genericType == ScrapsetTypes.None)
                    {
                        outputGate.GenericTypeReconciler.SetOutputGenericTypeMapping(outputParameterName, inputParameter.Type);
                        Debug.Log($"Output param '{outputParameterName}' of gate '{outputGate.GetType()}' with ID {outputGateId} had its generic identifier '{genericIdentifier}' set to {inputParameter} from the input param '{inputParameterName}' of gate '{inputGate.GetType()} with ID {inputGateId}'");
                    } else
                    {
                        Debug.Log($"Output param '{outputParameterName}' of gate '{outputGate.GetType()}' with ID {outputGateId} had its generic identifier '{genericIdentifier}' inferred as {genericType} from a previous connection involving generic identifier '{genericIdentifier}'");
                    }
                }
            }

            var link = new GateLink()
            {
                OutputGateId = outputGateId,
                OutputParameterName = outputParameterName,
                InputGateId = inputGateId,
                InputParameterName = inputParameterName,
            };


            Dictionary<string, GateLink> outLinksByInputParam;
            if (linksByGateIdInputParam.TryGetValue(inputGateId, out outLinksByInputParam))
            {
                // the gate already contains an input param dict (presumably because it has other inputs already linked),
                //  but does not contain a link for this input, so just add a new link to the dict for the input
                outLinksByInputParam.Add(inputParameterName, link);
            } else
            {
                // create a linksByInputParam dict for the gate and add the new link to the new dict
                Dictionary<string, GateLink> newLinksByInputParam = new Dictionary<string, GateLink>();
                newLinksByInputParam.Add(inputParameterName, link);
                linksByGateIdInputParam.Add(inputGateId, newLinksByInputParam);
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
            if (linkList.Any(u => u.InputParameterName == inputParameterName && u.InputGateId == inputGateId))
            {
                // The rationale here is that an output can serve as a data source for any number of inputs, but an input can only accept data from a
                //  single source. We determined in the validations above that the input doesn't have a source, so this could only result as a bug.
                //  That's why I'm keeping it as an actual exception here. It should not occur.
                throw new System.Exception($"The output list for gate ID {outputGateId} already contains an entry for input param" +
                    $"'{inputParameterName}' of gate ID {inputGateId}. This is likely a bug in the SubroutineInstance class.");
            } else
            {
                linkList.Add(link);
            }

            Debug.Log($"Linked gate '{outputGate.GetType()}' output '{outputParameterName}' to gate '{inputGate.GetType()}' input '{inputParameterName}'");
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

            Debug.Log($"Removed link for gate ID {outputGateId} output '{outputParameterName}' and gate ID {inputGateId} input '{inputParameterName}'");
        }

        public void RemoveAllInputOutputLinks(int gateId)
        {
            var gate = subroutineDefinition.GetGateById(gateId);
            foreach (var input in gate.InputParameters)
            {
                RemoveInputOutputLink(gateId, input.Key);
            }

            // remove all links from this gate's outputs
            if (linksByGateIdOutputParam.ContainsKey(gateId))
            {
                foreach (var kv in linksByGateIdOutputParam[gateId])
                {
                    var gateLinks = kv.Value;

                    for (var i = gateLinks.Count - 1; i >= 0; i--)
                    {
                        RemoveInputOutputLink(gateLinks[i].InputGateId, gateLinks[i].InputParameterName);
                    }
                }

                linksByGateIdOutputParam[gateId] = null;
            }

            Debug.Log($"Removed all I/O links for gate '{gate.GetType()}' with ID {gate.Id}");
        }
    }
}
