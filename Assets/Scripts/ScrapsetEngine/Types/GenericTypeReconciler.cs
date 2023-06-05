using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class GenericTypeReconciler
    {
        // dictionary of generic identifiers to inferred type
        private Dictionary<string, ScrapsetTypes> inferredTypes = new Dictionary<string, ScrapsetTypes>();
        // dictionary of input param names to generic identifiers
        private Dictionary<string, string> genericInputIdentifiers { get; set; } = new Dictionary<string, string>();
        // dictionary of output param names to generic identifiers
        private Dictionary<string, string> genericOutputIdentifiers { get; set; } = new Dictionary<string, string>();

        public ScrapsetTypes GetTypeOfGenericIdentifier(string identifier)
        {
            if (!inferredTypes.TryGetValue(identifier, out var scrapsetType))
            {
                return ScrapsetTypes.None;
            }

            return scrapsetType;
        }

        public string GetGenericIdentifierOfInputParam(string inputParamName)
        {
            if (!genericInputIdentifiers.TryGetValue(inputParamName, out var identifier))
            {
                return null;
            }

            return identifier;
        }

        public string GetGenericIdentifierOfOutputParam(string outputParamName)
        {
            if (!genericOutputIdentifiers.TryGetValue(outputParamName, out var identifier))
            {
                return null;
            }

            return identifier;
        }

        public void AssignGenericIdentifierToInput(string inputParamName, string identifier)
        {
            genericInputIdentifiers[inputParamName] = identifier;
        }

        public void AssignGenericIdentifierToOutput(string outputParamName, string identifier)
        {
            genericOutputIdentifiers[outputParamName] = identifier;
        }

        public void SetInputGenericTypeMapping(string inputParamName, ScrapsetTypes scrapsetType)
        {
            if (!genericInputIdentifiers.TryGetValue(inputParamName, out var identifier))
            {
                throw new System.Exception($"Could not set generic type mapping: input param '{inputParamName}' is not generic");
            }

            inferredTypes[identifier] = scrapsetType;
        }

        public void SetOutputGenericTypeMapping(string outputParamName, ScrapsetTypes scrapsetType)
        {
            if (!genericOutputIdentifiers.TryGetValue(outputParamName, out var identifier))
            {
                throw new System.Exception($"Could not set generic type mapping: output param '{outputParamName}' is not generic");
            }

            inferredTypes[identifier] = scrapsetType;
        }
    }
}
