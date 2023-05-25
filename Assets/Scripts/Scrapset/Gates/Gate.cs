using System.Collections.Generic;
using UnityEngine;

public abstract class Gate : MonoBehaviour, IGate
{
    abstract public string Name { get; }

    abstract public string Description { get; }

    abstract public string Category { get; }

    public Dictionary<string, ScrapsetTypes> InputParameters { get; private set; } = new Dictionary<string, ScrapsetTypes>();
    public Dictionary<string, ScrapsetTypes> OutputParameters { get; private set; } = new Dictionary<string, ScrapsetTypes>();
    public GenericTypeReconciler GenericTypeReconciler { get; private set; } = new GenericTypeReconciler();

    public int Id { get; set; }

    public ScrapsetTypes GetInputParameter(string parameterName)
    {
        if (!InputParameters.TryGetValue(parameterName, out var parameterType))
        {
            return ScrapsetTypes.None;
        }

        return parameterType;
    }

    public ScrapsetTypes GetOutputParameter(string parameterName)
    {
        if (!OutputParameters.TryGetValue(parameterName, out var parameterType))
        {
            return ScrapsetTypes.None;
        }

        return parameterType;
    }
}
