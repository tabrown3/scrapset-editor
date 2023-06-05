using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class LessThanGate : Gate, IExpression
    {
        public override string Name => "Less Than";

        public override string Description => "Compares inputs A and B and returns true if A is less than B";

        public override LanguageCategory Category { get; set; } = LanguageCategory.Expression;

        public LessThanGate()
        {
            InputParameters.Add("A", new InputParameter() { Type = ScrapsetTypes.Number });
            InputParameters.Add("B", new InputParameter() { Type = ScrapsetTypes.Number });
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Bool });
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            var a = (float)ScrapsetValue.GetDefaultForType(ScrapsetTypes.Number);
            if (inputs.ContainsKey("A"))
            {
                a = (float)inputs["A"].Value;
            }

            var b = (float)ScrapsetValue.GetDefaultForType(ScrapsetTypes.Number);
            if (inputs.ContainsKey("B"))
            {
                b = (float)inputs["B"].Value;
            }

            var result = a < b;
            var outVal = new ScrapsetValue(ScrapsetTypes.Bool);
            outVal.Value = result;
            return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
        }
    }
}
