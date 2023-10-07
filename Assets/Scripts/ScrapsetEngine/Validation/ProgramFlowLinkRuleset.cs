namespace Scrapset.Engine
{
    public class ProgramFlowLinkRuleset
    {
        public ValidationResult ValidateLinkCreation(SubroutineDefinition subroutineDefinition, int fromId, string flowName, int toId)
        {
            var validator = new ProgramLinkCreationValidator(subroutineDefinition, fromId, flowName, toId);

            return validator.Validate();
        }
    }
}
