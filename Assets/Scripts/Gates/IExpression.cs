using System.Collections.Generic;

public interface IExpression
{
    Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs);
}
