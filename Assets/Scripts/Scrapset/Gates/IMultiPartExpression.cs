using System.Collections.Generic;
using static SubroutineInstance;

public interface IMultiPartExpression
{
    Dictionary<string, ScrapsetValue> EvaluateMultiPart(Dictionary<string, LazyEvaluateDependency> evalCallbacks);
}
