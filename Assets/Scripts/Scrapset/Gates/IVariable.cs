using System.Collections.Generic;

public interface IVariable
{
    public string VariableName { get; set; }
    public ScrapsetValue Read(Dictionary<string, ScrapsetValue> variableStore);

    public void Write(ScrapsetValue inVal, Dictionary<string, ScrapsetValue> variableStore);
}
