using System.Collections.Generic;
using UnityEngine;

public class StringVariableGate : Gate, IVariable, IExpression
{
    override public string Name => "String Variable";

    override public string Description => "Stores a value of type String";

    override public string Category => "Variables";

    public ScrapsetValue Reference { get; set; }
    public string VariableName { get; set; }

    public StringVariableGate()
    {
        InputParameters.Add("In", ScrapsetTypes.String);
        OutputParameters.Add("Out", ScrapsetTypes.String);
    }

    public ScrapsetValue Read()
    {
        var outVal = new ScrapsetValue(ScrapsetTypes.String);
        outVal.Value = Reference.Value;
        return outVal;
    }

    public void Write(ScrapsetValue inVal)
    {
        if (inVal.Type != ScrapsetTypes.String)
        {
            throw new System.Exception("Cannot write value to StringVariable: must be of Scrapset type String");
        }

        if (inVal.Value == null)
        {
            throw new System.Exception("Cannot write value to StringVariable: value cannot be null");
        }

        var sInVal = (string)inVal.Value;

        Reference.Value = sInVal;

        Debug.Log($"Wrote value {Reference.Value} to variable gate '{Name}'");
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        return new Dictionary<string, ScrapsetValue>() { { "Out", Read() } };
    }
}
