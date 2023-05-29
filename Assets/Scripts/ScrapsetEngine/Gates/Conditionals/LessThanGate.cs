using System.Collections.Generic;

public class LessThanGate : Gate, IExpression
{
    public override string Name => "Less Than";

    public override string Description => "Compares inputs A and B and returns true if A is less than B";

    public override LanguageCategory Category => LanguageCategory.Expression;

    public LessThanGate()
    {
        InputParameters.Add("A", ScrapsetTypes.Number);
        InputParameters.Add("B", ScrapsetTypes.Number);
        OutputParameters.Add("Out", ScrapsetTypes.Bool);
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        var a = (float)ScrapsetValue.GetDefaultForType(ScrapsetTypes.Number);
        if (inputs.ContainsKey("A"))
        {
            a = (float)inputs["A"].Value;
        }

        var b = (float)ScrapsetValue.GetDefaultForType(ScrapsetTypes.Number);
        if (inputs.ContainsKey("B"))
        {
            b = (float)inputs["B"].Value;
        }

        var result = a < b;
        var outVal = new ScrapsetValue(ScrapsetTypes.Bool);
        outVal.Value = result;
        return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
    }
}
