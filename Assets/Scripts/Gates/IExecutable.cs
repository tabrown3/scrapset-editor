using System.Collections.Generic;

public interface IExecutable
{
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }
    public Dictionary<string, ScrapsetTypes> Inputs { get; }
    public Dictionary<string, ScrapsetTypes> Outputs { get; }
    public int Id { get; set; }
    public Dictionary<string, ScrapsetValue> Evaluate(Dictionary<string, ScrapsetValue> inputs);
}
