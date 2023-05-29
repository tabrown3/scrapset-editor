using System.Collections.Generic;

public class SubroutineExpressionGate : Gate, IExpression
{
    public override string Name => subroutineDefinition.Name;

    public override string Description => $"An instance of '{Name}' (subroutine)";

    override public LanguageCategory Category => LanguageCategory.Subroutine;

    SubroutineDefinition subroutineDefinition;

    public SubroutineExpressionGate(SubroutineDefinition _subroutineDefinition)
    {
        subroutineDefinition = _subroutineDefinition;
        InputParameters = _subroutineDefinition.InputParameters;
        OutputParameters = _subroutineDefinition.OutputParameters;
    }

    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs)
    {
        var instance = new SubroutineInstance();
        instance.SubroutineDefinition = subroutineDefinition;
        return instance.Execute(inputs);
    }
}
