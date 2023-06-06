using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class NumberConstantValueGate : Gate, IExpression
    {
        float constantValue = 1f;

        public NumberConstantValueGate()
        {
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Number });
        }

        public NumberConstantValueGate(float _constantValue) : this()
        {
            constantValue = _constantValue;
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            var outVal = new ScrapsetValue(ScrapsetTypes.Number);
            outVal.Value = constantValue;
            return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
        }
    }
}
