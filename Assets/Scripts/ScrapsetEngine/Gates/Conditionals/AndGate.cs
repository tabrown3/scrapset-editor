using System.Collections.Generic;

public class AndGate : Gate, IMultiPartExpression
{
    public override string Name => "And";

    public override string Description => "Performs logical AND on A and B";

    public override LanguageCategory Category { get; set; } = LanguageCategory.Expression;

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

        // only eval B if A is true
        outVal.Value = (bool)evalCallbacks["A"]().Value && (bool)evalCallbacks["B"]().Value;

        outDict["Out"] = outVal;
        return outDict;
    }
}
