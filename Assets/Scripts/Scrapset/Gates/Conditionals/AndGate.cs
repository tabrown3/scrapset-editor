using System.Collections.Generic;

public class AndGate : Gate, IMultiPartExpression
{
    public override string Name => throw new System.NotImplementedException();

    public override string Description => throw new System.NotImplementedException();

    public override string Category => throw new System.NotImplementedException();

    public AndGate()
    {
        InputParameters.Add("A", ScrapsetTypes.Bool);
        InputParameters.Add("B", ScrapsetTypes.Bool);
        OutputParameters.Add("Out", ScrapsetTypes.Bool);
    }

    public IEnumerable<IList<string>> GetParts()
    {
        var outerList = new List<IList<string>>();
        outerList.Add(new List<string>() { "A" });
        outerList.Add(new List<string>() { "B" });
        return outerList;
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        throw new System.NotImplementedException();
    }
}
