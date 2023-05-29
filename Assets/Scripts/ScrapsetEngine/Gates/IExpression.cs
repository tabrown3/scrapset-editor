using System.Collections.Generic;

namespace Scrapset.Engine
{
    public interface IExpression
    {
        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs);
    }
}
