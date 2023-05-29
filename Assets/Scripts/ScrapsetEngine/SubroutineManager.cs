using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Engine
{
    public class SubroutineManager : MonoBehaviour
    {
        Dictionary<string, SubroutineDefinition> subroutineDefinitions = new Dictionary<string, SubroutineDefinition>();

        public bool HasDefinition(string name)
        {
            return subroutineDefinitions.ContainsKey(name);
        }

        public SubroutineDefinition GetDefinition(string name)
        {
            if (!subroutineDefinitions.TryGetValue(name, out var subroutineDefinition))
            {
                throw new System.Exception($"Could not find subroutine defintion '{name}'");
            }

            Debug.Log($"Got definition for subroutine '{name}'");
            return subroutineDefinition;
        }

        public SubroutineDefinition DeclareSubroutine(string name)
        {
            var subroutineDefinition = new SubroutineDefinition(name, this);

            if (subroutineDefinitions.ContainsKey(name))
            {
                throw new System.Exception($"Could not declare subroutine: a subroutine named '{name}' already exists");
            }

            subroutineDefinitions[name] = subroutineDefinition;

            Debug.Log($"Declared subroutine '{name}'");
            return subroutineDefinition;
        }

        public SubroutineDefinition DeleteDefinition(string name)
        {
            if (!subroutineDefinitions.TryGetValue(name, out var subroutineDefinition))
            {
                throw new System.Exception($"Could not delete subroutine defintion '{name}': definition does not exist");
            }

            subroutineDefinitions.Remove(name);

            Debug.Log($"Deleted subroutine '{name}'");
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

            Debug.Log($"Created subroutine instance for '{name}'");
            return instance;
        }

        public Dictionary<string, ScrapsetValue> CallSubroutine(string name, Dictionary<string, ScrapsetValue> inputArgs)
        {
            var instance = CreateInstance(name);

            Debug.Log($"Called subroutine '{name}'");
            return instance.Execute(inputArgs);
        }
    }
}