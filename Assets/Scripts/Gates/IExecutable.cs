using System.Collections.Generic;

public interface IExecutable
{
    string Name { get; }
    int Id { get; }
    string Description { get; }
    string Category { get; }
    Dictionary<string, ScrapsetType> Inputs { get; }
    Dictionary<string, ScrapsetType> Outputs { get; }
}
