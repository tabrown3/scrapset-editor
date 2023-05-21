using System.Collections.Generic;
using UnityEngine;

public class NumberVariableGate : MonoBehaviour, IGate, IVariable
{
    public string Name => "Number Variable";

    public string Description => "Stores a value of type Number";

    public string Category => "Variables";

    public Dictionary<string, ScrapsetTypes> InputParameters => new Dictionary<string, ScrapsetTypes>() { { "In", ScrapsetTypes.Number } };

    public Dictionary<string, ScrapsetTypes> OutputParameters => new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Number } };

    public int Id { get; set; }

    private float value;

    public ScrapsetValue Read()
    {
        var outVal = new ScrapsetValue(ScrapsetTypes.Number);
        outVal.Value = value;
        return outVal;
    }

    public void Write(ScrapsetValue inVal)
    {
        if (inVal.Type != ScrapsetTypes.Number)
        {
            throw new System.Exception("Cannot write value to NumberVariable: must be of Scrapset type Number");
        }

        if (inVal.Value == null)
        {
            throw new System.Exception("Cannot write value to NumberVariable: value cannot be null");
        }

        var fInVal = (float)inVal.Value;

        value = fInVal;
    }
}
