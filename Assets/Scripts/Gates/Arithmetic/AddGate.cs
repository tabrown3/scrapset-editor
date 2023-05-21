using System.Collections.Generic;
using UnityEngine;

public class AddGate : MonoBehaviour, IExecutable
{
    public string Name => "Add";

    public string Description => "Adds numbers together";

    public string Category => "Arithmetic";

    public Dictionary<string, ScrapsetTypes> Inputs { get; } = new Dictionary<string, ScrapsetTypes>() { { "A", ScrapsetTypes.Number }, { "B", ScrapsetTypes.Number } };

    public Dictionary<string, ScrapsetTypes> Outputs { get; } = new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Number } };

    public int Id { get; set; }

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
