using System.Collections.Generic;

public interface IMultiPartExpression : IExpression
{
    IEnumerable<IList<string>> GetParts();
}
