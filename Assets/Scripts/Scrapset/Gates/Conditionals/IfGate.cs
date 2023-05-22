using System.Collections.Generic;

public class IfGate : Gate, IStatement
{
    public override string Name => "If";

    public override string Description => "Branches one way or the other based whether the input is True or False";

    public override string Category => "Conditionals";

    public List<string> OutwardPaths { get; set; } = new List<string>() { "Then", "Else" };

    public IfGate()
    {
        InputParameters.Add("Condition", ScrapsetTypes.Bool);
    }

    public void PerformSideEffect(Processor processor)
    {
        var result = processor.GetCachedInputValue(Id, "Condition");
        var isTrue = (bool)result.Value;
        if (isTrue)
        {
            processor.Goto(this, "Then");
        } else
        {
            processor.Goto(this, "Else");
        }
    }
}
