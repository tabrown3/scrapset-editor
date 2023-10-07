namespace Scrapset.Engine
{
    public class ProgramLinkCreationValidator : IScrapsetValidator
    {
        SubroutineDefinition subroutineDefinition;
        int fromId;
        string flowName;
        int toId;

        public ProgramLinkCreationValidator(SubroutineDefinition subroutineDefinition, int fromId, string flowName, int toId)
        {
            this.subroutineDefinition = subroutineDefinition;
            this.fromId = fromId;
            this.flowName = flowName;
            this.toId = toId;
        }

        public ValidationResult Validate()
        {
            var fromGate = subroutineDefinition.GetGateById(fromId);
            if (fromGate == null)
            {
                return new ValidationResult($"Gate with ID {fromId} not found", ValidationErrorCode.FromGateNotFound);
            }

            var toGate = subroutineDefinition.GetGateById(toId);
            if (toGate == null)
            {
                return new ValidationResult($"Gate with ID {toId} not found", ValidationErrorCode.ToGateNotFound);
            }

            IStatement fromStatement = fromGate as IStatement;
            if (fromStatement == null)
            {
                return new ValidationResult($"Gate with ID {fromId} does not implement IStatement", ValidationErrorCode.FromGateNotStatement);
            }

            IStatement toStatement = toGate as IStatement;
            if (toStatement == null)
            {
                return new ValidationResult($"Gate with ID {toId} does not implement IStatement", ValidationErrorCode.ToGateNotFound);
            }

            if (!fromStatement.OutwardPaths.Contains(flowName))
            {
                return new ValidationResult($"Gate with ID {fromId} does not have outward path of '${flowName}'", ValidationErrorCode.OutwardPathNameNotFound);
            }

            return new ValidationResult();
        }
    }
}
