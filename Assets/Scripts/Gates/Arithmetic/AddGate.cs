using System.Collections.Generic;
using UnityEngine;

public class AddGate : MonoBehaviour, IExecutable, IExpression
{
    public string Name => "Add";

    public int Id => 0;

    public string Description => "Adds numbers together";

    public string Category => "Arithmetic";

    public Dictionary<string, ScrapsetType> Inputs { get; } = new Dictionary<string, ScrapsetType>();

    public Dictionary<string, ScrapsetType> Outputs { get; } = new Dictionary<string, ScrapsetType>();

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        return new Dictionary<string, ScrapsetValue>();
    }
}
