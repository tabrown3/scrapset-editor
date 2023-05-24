using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GateIORegistry
{
    // linksByGateIdInputParam is a deep Dictionary storing all the I/O links by the calling gate's ID and input param name.
    //  Outer key is IGate.Id, inner key is InputParameterName
    Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam = new Dictionary<int, Dictionary<string, GateLink>>();
    // linksByGateIdOutputParam is a deep Dictionary storing all the I/O links by the source (outputting) gate's ID and output param name.
    //  Outer key is IGate.Id, inner key is InputParameterName, value is a list of all GateLinks for which this output is a data source
    Dictionary<int, Dictionary<string, List<GateLink>>> linksByGateIdOutputParam = new Dictionary<int, Dictionary<string, List<GateLink>>>();

    Processor parentProcessor;

    public GateIORegistry(Processor processor)
    {
        parentProcessor = processor;
    }

    public bool HasInputLinks(int gateId)
    {
        if (!linksByGateIdInputParam.TryGetValue(gateId, out var linksByInputParam))
        {
            return false;
        }

        return linksByInputParam != null && linksByInputParam.Count > 0;
    }

    public IList<KeyValuePair<string, GateLink>> GetInputLinks(int gateId)
    {
        if (HasInputLinks(gateId))
        {
            return linksByGateIdInputParam[gateId].AsReadOnlyList();
        }

        return new List<KeyValuePair<string, GateLink>>();
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
        var outputGate = parentProcessor.FindGateById(outputGateId);
        if (outputGate == null)
        {
            throw new System.Exception($"Could not find gate with ID ${outputGateId}");
        }

        var inputGate = parentProcessor.FindGateById(inputGateId);
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

        Debug.Log($"Linked gate '{outputGate.Name}' output '{outputParameterName}' to gate '{inputGate.Name}' input '{inputParameterName}'");
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
        var gate = parentProcessor.FindGateById(gateId);
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

        Debug.Log($"Removed all I/O links for gate '{gate.Name}' with ID {gate.Id}");
    }
}
