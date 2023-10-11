namespace Scrapset.Engine
{
    public class GateIOLinkRuleset
    {
        public static ValidationResult<GateIOLinkCreationComputedValues> ValidateLinkCreation(
            SubroutineDefinition subroutineDefinition,
            int inputGateId,
            string inputParameterName,
            int outputGateId,
            string outputParameterName)
        {
            var validator = new GateIOLinkCreationValidator(subroutineDefinition, inputGateId, inputParameterName, outputGateId, outputParameterName);

            return validator.Validate();
        }
    }
}
