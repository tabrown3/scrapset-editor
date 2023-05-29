using System.Collections.Generic;

public class AddGate : Gate, IExpression
{
    override public string Name => "Add";

    override public string Description => "Adds numbers together";

    override public LanguageCategory Category { get; set; } = LanguageCategory.Expression;

    public AddGate()
    {
        InputParameters.Add("A", ScrapsetTypes.Number);
        InputParameters.Add("B", ScrapsetTypes.Number);
        OutputParameters.Add("Out", ScrapsetTypes.Number);
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        ScrapsetValue arg1;
        float val1 = 0f;
        if (inputs.TryGetValue("A", out arg1) && arg1.Type == ScrapsetTypes.Number)
        {
            val1 = (float)arg1.Value;
        }

        ScrapsetValue arg2;
        float val2 = 0f;
        if (inputs.TryGetValue("B", out arg2) && arg2.Type == ScrapsetTypes.Number)
        {
            val2 = (float)arg2.Value;
        }

        float sum = val1 + val2;
        var outVal = new ScrapsetValue(ScrapsetTypes.Number);
        outVal.Value = sum;

        return new Dictionary<string, ScrapsetValue>() {
            { "Out", outVal }
        };
    }
}
