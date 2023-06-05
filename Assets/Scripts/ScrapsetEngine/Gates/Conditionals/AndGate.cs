using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class AndGate : Gate, IExpression
    {
        public override string Name => "And";

        public override string Description => "Performs logical AND on A and B";

        public override LanguageCategory Category { get; set; } = LanguageCategory.Expression;

        public AndGate()
        {
            InputParameters.Add("A", new InputParameter() { Type = ScrapsetTypes.Bool, IsDeferred = true });
            InputParameters.Add("B", new InputParameter() { Type = ScrapsetTypes.Bool, IsDeferred = true });
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Bool });
        }

        // allows for short circuiting, you must pass in callbacks that evaluate one of this gate's deps when executed
        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            var outDict = new Dictionary<string, ScrapsetValue>();
            var outVal = new ScrapsetValue(ScrapsetTypes.Bool);

            // only eval B if A is true
            outVal.Value = (bool)deferredInputs["A"]().Value && (bool)deferredInputs["B"]().Value;

            outDict["Out"] = outVal;
            return outDict;
        }
    }
}
