using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class BoolConstantValueGate : Gate, IExpression
    {
        bool constantValue = false;

        override public string Name => "Bool Constant Value";

        override public string Description => "Returns a constant value";

        override public LanguageCategory Category { get; set; } = LanguageCategory.Variable;

        public BoolConstantValueGate()
        {
            OutputParameters.Add("Out", ScrapsetTypes.Bool);
        }

        public BoolConstantValueGate(bool _constantValue) : this()
        {
            constantValue = _constantValue;
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
        {
            var outVal = new ScrapsetValue(ScrapsetTypes.Bool);
            outVal.Value = constantValue;

            return new Dictionary<string, ScrapsetValue>() { { "Out", outVal } };
        }
    }
}
