using System.Collections.Generic;

namespace Scrapset.Engine
{
    public abstract class Gate : IGate
    {
        public int Id { get; set; }

        public Dictionary<string, InputParameter> InputParameters { get; protected set; } = new Dictionary<string, InputParameter>();
        public Dictionary<string, OutputParameter> OutputParameters { get; protected set; } = new Dictionary<string, OutputParameter>();
        public GenericTypeReconciler GenericTypeReconciler { get; private set; } = new GenericTypeReconciler();

        public InputParameter GetInputParameter(string parameterName)
        {
            if (!InputParameters.TryGetValue(parameterName, out var parameter))
            {
                return new InputParameter() { Type = ScrapsetTypes.None };
            }

            return parameter;
        }

        public OutputParameter GetOutputParameter(string parameterName)
        {
            if (!OutputParameters.TryGetValue(parameterName, out var parameter))
            {
                return new OutputParameter() { Type = ScrapsetTypes.None };
            }

            return parameter;
        }
    }
}
