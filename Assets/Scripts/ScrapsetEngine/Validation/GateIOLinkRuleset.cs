using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class GateIOLinkRuleset
    {
        public static ValidationResult<GateIOLinkCreationComputedValues> ValidateLinkCreation(
            Dictionary<int, Dictionary<string, GateLink>> linksByGateIdInputParam,
            SubroutineDefinition subroutineDefinition,
            int inputGateId,
            string inputParameterName,
            int outputGateId,
            string outputParameterName)
        {
            var validator = new GateIOLinkCreationValidator(linksByGateIdInputParam, subroutineDefinition, inputGateId, inputParameterName, outputGateId, outputParameterName);

            return validator.Validate();
        }
    }
}
