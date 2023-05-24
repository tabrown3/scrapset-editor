using System.Collections.Generic;

public class GateLinks
{
    // linksByGateIdInputParam is a deep Dictionary storing all the I/O links by the calling gate's ID and input param name.
    //  Outer key is IGate.Id, inner key is InputParameterName
    public Dictionary<string, GateLink> Inputs = new Dictionary<string, GateLink>();
    // linksByGateIdOutputParam is a deep Dictionary storing all the I/O links by the source (outputting) gate's ID and output param name.
    //  Outer key is IGate.Id, inner key is InputParameterName, value is a list of all GateLinks for which this output is a data source
    public Dictionary<string, List<GateLink>> Outputs = new Dictionary<string, List<GateLink>>();
}
