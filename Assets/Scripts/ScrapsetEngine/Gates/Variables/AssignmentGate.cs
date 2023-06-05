using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class AssignmentGate : Gate, IStatement
    {
        public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

        override public string Name => "Assignment";

        override public string Description => "Assigns values to variables";

        override public LanguageCategory Category { get; set; } = LanguageCategory.Statement;

        public AssignmentGate()
        {
            InputParameters.Add("In", new InputParameter() { Type = ScrapsetTypes.Generic });
            GenericTypeReconciler.AssignGenericIdentifierToInput("In", "T");
            OutputParameters.Add("Out", new OutputParameter() { Type = ScrapsetTypes.Generic });
            GenericTypeReconciler.AssignGenericIdentifierToOutput("Out", "T");
        }

        public void PerformSideEffect(SubroutineInstance instance)
        {
            instance.AssignInputToOutput(this, "In", "Out");
            instance.Goto(this, "Next");
        }
    }
}
