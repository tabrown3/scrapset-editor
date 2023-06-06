using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class IfGate : Gate, IStatement
    {
        public List<string> OutwardPaths { get; set; } = new List<string>() { "Then", "Else" };

        public IfGate()
        {
            InputParameters.Add("Condition", new InputParameter() { Type = ScrapsetTypes.Bool });
        }

        public void PerformSideEffect(SubroutineInstance instance)
        {
            var result = instance.GetCachedInputValue(Id, "Condition");
            var isTrue = (bool)result.Value;
            if (isTrue)
            {
                instance.Goto(this, "Then");
            } else
            {
                instance.Goto(this, "Else");
            }
        }
    }
}
