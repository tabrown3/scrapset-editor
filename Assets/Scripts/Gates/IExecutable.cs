using System.Collections.Generic;

public interface IExecutable
{
    string Name { get; }
    string Description { get; }
    string Category { get; }
    Dictionary<string, ScrapsetTypes> Inputs { get; }
    Dictionary<string, ScrapsetTypes> Outputs { get; }
}
