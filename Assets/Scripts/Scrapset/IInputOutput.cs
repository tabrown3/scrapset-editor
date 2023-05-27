using System.Collections.Generic;

public interface IInputOutput
{
    public Dictionary<string, ScrapsetTypes> InputParameters { get; }
    public Dictionary<string, ScrapsetTypes> OutputParameters { get; }
    public GenericTypeReconciler GenericTypeReconciler { get; }

    public ScrapsetTypes GetInputParameter(string parameterName);

    public ScrapsetTypes GetOutputParameter(string parameterName);
}
