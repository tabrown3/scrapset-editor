using System.Collections.Generic;

public class ConstantValueGate : Gate, IExpression
{
    float constantValue = 1f;

    override public string Name => "Constant Value";

    override public string Description => "Returns a constant value";

    override public string Category => "Variables";

    public ConstantValueGate()
    {
        OutputParameters.Add("Out", ScrapsetTypes.Number);
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        var outVal = new ScrapsetValue(ScrapsetTypes.Number);
        outVal.Value = constantValue;
        return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
    }
}
