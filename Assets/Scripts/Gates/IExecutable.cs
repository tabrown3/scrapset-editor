using System.Collections.Generic;

public interface IExecutable
{
    static string Name { get; }
    static string Description { get; }
    static string Category { get; }
    static Dictionary<string, ScrapsetTypes> Inputs { get; }
    static Dictionary<string, ScrapsetTypes> Outputs { get; }
    public int Id { get; set; }
}
