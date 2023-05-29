using System.Collections.Generic;
using UnityEngine;

public class SubroutineManager : MonoBehaviour
{
    Dictionary<string, SubroutineDefinition> subroutineDefinitions = new Dictionary<string, SubroutineDefinition>();

    public SubroutineDefinition GetDefinition(string name)
    {
        if (!subroutineDefinitions.TryGetValue(name, out var subroutineDefinition))
        {
            throw new System.Exception($"Could not find subroutine defintion '{name}'");
        }

        return subroutineDefinition;
    }

    public SubroutineDefinition DeclareSubroutine(string name)
    {
        var subroutineDefinition = new SubroutineDefinition(name, this);
        subroutineDefinitions[name] = subroutineDefinition;
        return subroutineDefinition;
    }

    public SubroutineDefinition DeleteDefinition(string name)
    {
        if (!subroutineDefinitions.TryGetValue(name, out var subroutineDefinition))
        {
            throw new System.Exception($"Could not delete subroutine defintion '{name}': definition does not exist");
        }

        subroutineDefinitions.Remove(name);

        return subroutineDefinition;
    }

    public IReadOnlyDictionary<string, SubroutineDefinition> GetAllDefinitions()
    {
        return subroutineDefinitions;
    }

    public SubroutineInstance CreateInstance(string name)
    {
        var definition = GetDefinition(name);
        var instance = new SubroutineInstance();
        instance.SubroutineDefinition = definition;
        return instance;
    }

    public Dictionary<string, ScrapsetValue> CallSubroutine(string name, Dictionary<string, ScrapsetValue> inputArgs)
    {
        var instance = CreateInstance(name);
        return instance.Execute(inputArgs);
    }
}