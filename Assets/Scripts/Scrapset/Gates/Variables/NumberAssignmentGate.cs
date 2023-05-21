using System.Collections.Generic;

public class NumberAssignmentGate : Gate, IStatement
{
    public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

    override public string Name => "Number Assignment";

    override public string Description => "Assigns values to variables";

    override public string Category => "Variables";

    public NumberAssignmentGate()
    {
        InputParameters.Add("In", ScrapsetTypes.Number);
        OutputParameters.Add("Out", ScrapsetTypes.Number);
    }

    public void PerformSideEffect(Processor processor)
    {
        processor.AssignInputToOutput(this, "In", "Out");
        processor.Goto(this, "Next");
    }
}
