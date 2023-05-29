using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class Entrypoint : Gate, IStatement
    {
        public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

        override public string Name => "Entrypoint";

        override public string Description => "The program's starting point";

        override public LanguageCategory Category { get; set; } = LanguageCategory.Entrypoint;

        public Entrypoint()
        {

        }

        public void PerformSideEffect(SubroutineInstance instance)
        {
            instance.Goto(this, "Next");
        }
    }
}
