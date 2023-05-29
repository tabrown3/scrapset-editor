using System.Collections.Generic;

public class IfGate : Gate, IStatement
{
    public override string Name => "If";

    public override string Description => "Branches one way or the other based on whether the input is True or False";

    public override LanguageCategory Category { get; set; } = LanguageCategory.Statement;

    public List<string> OutwardPaths { get; set; } = new List<string>() { "Then", "Else" };

    public IfGate()
    {
        InputParameters.Add("Condition", ScrapsetTypes.Bool);
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
