using System.Collections.Generic;

public interface IGate
{
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }
    public Dictionary<string, ScrapsetTypes> InputParameters { get; }
    public Dictionary<string, ScrapsetTypes> OutputParameters { get; }
    public int Id { get; set; }

    public ScrapsetTypes GetInputParameter(string parameterName);

    public ScrapsetTypes GetOutputParameter(string parameterName);
}
