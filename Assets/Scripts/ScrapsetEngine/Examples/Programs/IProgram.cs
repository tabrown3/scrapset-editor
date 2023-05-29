using System.Collections.Generic;

namespace Scrapset.Examples
{
    public interface IProgram
    {
        void Build();
        Dictionary<string, ScrapsetValue> Run();
    }
}
