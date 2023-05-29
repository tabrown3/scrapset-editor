using System.Collections.Generic;
using static Scrapset.Engine.SubroutineInstance;

namespace Scrapset.Engine
{
    public interface IMultiPartExpression
    {
        Dictionary<string, ScrapsetValue> EvaluateMultiPart(Dictionary<string, LazyEvaluateDependency> evalCallbacks);
    }
}
