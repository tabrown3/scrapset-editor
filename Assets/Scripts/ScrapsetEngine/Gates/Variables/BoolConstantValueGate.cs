using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class BoolConstantValueGate : Gate, IExpression
    {
        bool constantValue = false;

        public BoolConstantValueGate()
        {
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Bool });
        }

        public BoolConstantValueGate(bool _constantValue) : this()
        {
            constantValue = _constantValue;
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            var outVal = new ScrapsetValue(ScrapsetTypes.Bool);
            outVal.Value = constantValue;

            return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
        }
    }
}
