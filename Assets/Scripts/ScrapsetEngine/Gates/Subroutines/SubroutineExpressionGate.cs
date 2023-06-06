using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class SubroutineExpressionGate : Gate, IExpression
    {
        SubroutineDefinition subroutineDefinition;

        public SubroutineExpressionGate(SubroutineDefinition _subroutineDefinition)
        {
            subroutineDefinition = _subroutineDefinition;
            InputParameters = _subroutineDefinition.InputParameters;
            OutputParameters = _subroutineDefinition.OutputParameters;
        }

        public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs, Dictionary<string, SubroutineInstance.LazyEvaluateDependency> deferredInputs)
        {
            var instance = new SubroutineInstance();
            instance.SubroutineDefinition = subroutineDefinition;
            return instance.Execute(inputs);
        }
    }
}
