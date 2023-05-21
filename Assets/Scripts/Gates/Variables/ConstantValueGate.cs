using System.Collections.Generic;
using UnityEngine;

public class ConstantValueGate : MonoBehaviour, IExecutable, IExpression
{
    static public string Name => "Constant Value";

    static public string Description => "Returns a constant value";

    static public string Category => "Variables";

    static public Dictionary<string, ScrapsetTypes> Inputs { get; } = new Dictionary<string, ScrapsetTypes>();

    static public Dictionary<string, ScrapsetTypes> Outputs { get; } = new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Number } };

    public int Id { get; set; }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        var outVal = new ScrapsetValue(ScrapsetTypes.Number);
        outVal.Value = 4;
        return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
    }
}
