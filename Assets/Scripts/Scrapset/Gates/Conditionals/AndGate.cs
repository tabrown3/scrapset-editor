using System.Collections.Generic;

public class AndGate : Gate, IMultiPartExpression
{
    public override string Name => throw new System.NotImplementedException();

    public override string Description => throw new System.NotImplementedException();

    public override string Category => throw new System.NotImplementedException();

    public AndGate()
    {
        InputParameters.Add("A", ScrapsetTypes.Bool);
        InputParameters.Add("B", ScrapsetTypes.Bool);
        OutputParameters.Add("Out", ScrapsetTypes.Bool);
    }

    // allows for short circuiting, you must pass in callbacks that evaluate one of this gate's deps when executed
    public Dictionary<string, ScrapsetValue> EvaluateMultiPart(Dictionary<string, SubroutineInstance.LazyEvaluateDependency> evalCallbacks)
    {
        var outDict = new Dictionary<string, ScrapsetValue>();
        var outVal = new ScrapsetValue(ScrapsetTypes.Bool);

        var valA = (bool)evalCallbacks["A"]().Value;
        if (valA) // since A is true, we must evaluate B - the result will just be B
        {
            outVal.Value = (bool)evalCallbacks["B"]().Value;
        } else // since A is false, no reason to even evaluate B - the result is false (short-circuit)
        {
            outVal.Value = false;
        }

        outDict["Out"] = outVal;
        return outDict;
    }
}
