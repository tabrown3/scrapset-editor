using System.Collections.Generic;
using UnityEngine;

public class ConstantValueGate : MonoBehaviour, IExecutable
{
    public string Name => "Constant Value";

    public string Description => "Returns a constant value";

    public string Category => "Variables";

    public Dictionary<string, ScrapsetTypes> Inputs { get; } = new Dictionary<string, ScrapsetTypes>();

    public Dictionary<string, ScrapsetTypes> Outputs { get; } = new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Number } };

    public int Id { get; set; }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        var outVal = new ScrapsetValue(ScrapsetTypes.Number);
        outVal.Value = 4;
        return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
    }
}
