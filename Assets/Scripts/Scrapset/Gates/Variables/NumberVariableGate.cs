using System.Collections.Generic;
using UnityEngine;

public class NumberVariableGate : Gate, IVariable, IExpression
{
    override public string Name => "Number Variable";

    override public string Description => "Stores a value of type Number";

    override public string Category => "Variables";

    private float value;

    public NumberVariableGate()
    {
        InputParameters.Add("In", ScrapsetTypes.Number);
        OutputParameters.Add("Out", ScrapsetTypes.Number);
    }

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

        Debug.Log($"Wrote value {value} to variable gate '{Name}'");
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        return new Dictionary<string, ScrapsetValue>() { { "Out", Read() } };
    }
}
