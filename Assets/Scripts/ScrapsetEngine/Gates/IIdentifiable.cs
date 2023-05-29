using System.Collections.Generic;

namespace Scrapset.Engine
{
    public interface IIdentifiable
    {
        string Identifier { get; set; }
    }

    public interface IReadable
    {
        ScrapsetValue Read(Dictionary<string, ScrapsetValue> variableStore);
    }

    public interface IWritable
    {
        void Write(ScrapsetValue inVal, Dictionary<string, ScrapsetValue> variableStore);
    }
}
