using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class Entrypoint : Gate, IStatement
    {
        public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

        public Entrypoint()
        {

        }

        public void PerformSideEffect(SubroutineInstance instance)
        {
            instance.Goto(this, "Next");
        }
    }
}
