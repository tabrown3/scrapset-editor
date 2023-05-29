using System.Collections.Generic;

public class Entrypoint : Gate, IStatement
{
    public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

    override public string Name => "Entrypoint";

    override public string Description => "The program's starting point";

    override public LanguageCategory Category => LanguageCategory.Entrypoint;

    public Entrypoint()
    {

    }

    public void PerformSideEffect(SubroutineInstance instance)
    {
        instance.Goto(this, "Next");
    }
}
