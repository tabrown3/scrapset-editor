using System.Collections.Generic;

namespace Scrapset.Engine
{
    public interface IStatement
    {
        public List<string> OutwardPaths { get; set; }

        public void PerformSideEffect(SubroutineInstance instance);
    }
}
