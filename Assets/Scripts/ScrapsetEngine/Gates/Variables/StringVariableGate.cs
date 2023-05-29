using System.Collections.Generic;
using UnityEngine;

public class StringVariableGate : Gate, IIdentifiable, IReadable, IWritable, IExpression
{
    override public string Name => "String Variable";

    override public string Description => "Stores a value of type String";

    override public LanguageCategory Category { get; set; } = LanguageCategory.Variable;

    public string Identifier { get; set; }

    public StringVariableGate()
    {
        InputParameters.Add("In", ScrapsetTypes.String);
        OutputParameters.Add("Out", ScrapsetTypes.String);
    }

    // It might seem strange that you have to pass in the value store to get read by the variable,
    //  but gates DO NOT STORE their own state- not even variables.
    public ScrapsetValue Read(Dictionary<string, ScrapsetValue> variableStore)
    {
        if (!variableStore.ContainsKey(Identifier))
        {
            throw new System.Exception($"Cannot read from variable store: store does not contain an entry for variable '{Identifier}'");
        }

        return variableStore[Identifier];
    }

    public void Write(ScrapsetValue inVal, Dictionary<string, ScrapsetValue> variableStore)
    {
        if (inVal.Type != ScrapsetTypes.String)
        {
            throw new System.Exception("Cannot write value to StringVariable: must be of Scrapset type String");
        }

        if (inVal.Value == null)
        {
            throw new System.Exception("Cannot write value to StringVariable: value cannot be null");
        }

        if (!variableStore.ContainsKey(Identifier))
        {
            throw new System.Exception($"Cannot write to variable store: store does not contain an entry for variable '{Identifier}'");
        }

        var sInVal = (string)inVal.Value;

        Debug.Log($"Wrote value '{sInVal}' to variable '{Identifier}'");
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> variableStore)
    {
        return new Dictionary<string, ScrapsetValue>() { { "Out", Read(variableStore) } };
    }
}
