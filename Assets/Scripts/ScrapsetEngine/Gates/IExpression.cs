using System.Collections.Generic;

public interface IExpression
{
    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs);
}
