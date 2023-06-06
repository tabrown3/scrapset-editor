using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class SubtractGate : Gate, IExpression
    {
        public SubtractGate()
        {
            InputParameters.Add("A", new InputParameter() { Type = ScrapsetTypes.Number });
            InputParameters.Add("B", new InputParameter() { Type = ScrapsetTypes.Number });
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Number });
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            ScrapsetValue arg1;
            float val1 = 0f;
            if (inputs.TryGetValue("A", out arg1) && arg1.Type == ScrapsetTypes.Number)
            {
                val1 = (float)arg1.Value;
            }

            ScrapsetValue arg2;
            float val2 = 0f;
            if (inputs.TryGetValue("B", out arg2) && arg2.Type == ScrapsetTypes.Number)
            {
                val2 = (float)arg2.Value;
            }

            float difference = val1 - val2;
            var outVal = new ScrapsetValue(ScrapsetTypes.Number);
            outVal.Value = difference;

            return new Dictionary<string, ScrapsetValue>() {
            { "Out", outVal }
        };
        }
    }
}