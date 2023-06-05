using System.Collections.Generic;

namespace Scrapset.Engine
{
    public interface IInputOutput
    {
        public Dictionary<string, InputParameter> InputParameters { get; }
        public Dictionary<string, OutputParameter> OutputParameters { get; }
        public GenericTypeReconciler GenericTypeReconciler { get; }

        public InputParameter GetInputParameter(string parameterName);

        public OutputParameter GetOutputParameter(string parameterName);
    }
}
