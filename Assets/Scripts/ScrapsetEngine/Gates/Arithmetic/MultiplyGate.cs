using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class MultiplyGate : Gate, IExpression
    {
        override public string Name => "Multiply";

        override public string Description => "Multiplies numbers together";

        override public LanguageCategory Category { get; set; } = LanguageCategory.Expression;

        public MultiplyGate()
        {
            InputParameters.Add("A", ScrapsetTypes.Number);
            InputParameters.Add("B", ScrapsetTypes.Number);
            OutputParameters.Add("Out", ScrapsetTypes.Number);
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
        {
            ScrapsetValue arg1;
            float val1 = 1f;
            if (inputs.TryGetValue("A", out arg1) && arg1.Type == ScrapsetTypes.Number)
            {
                val1 = (float)arg1.Value;
            }

            ScrapsetValue arg2;
            float val2 = 1f;
            if (inputs.TryGetValue("B", out arg2) && arg2.Type == ScrapsetTypes.Number)
            {
                val2 = (float)arg2.Value;
            }

            float product = val1 * val2;
            var outVal = new ScrapsetValue(ScrapsetTypes.Number);
            outVal.Value = product;

            return new Dictionary<string, ScrapsetValue>() {
            { "Out", outVal }
        };
        }
    }
}
