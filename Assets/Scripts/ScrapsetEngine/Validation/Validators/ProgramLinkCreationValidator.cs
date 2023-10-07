namespace Scrapset.Engine
{
    public class ProgramLinkCreationValidator : IScrapsetValidator<ProgramLinkCreationComputedValues>
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

        public ValidationResult<ProgramLinkCreationComputedValues> Validate()
        {
            var fromGate = subroutineDefinition.GetGateById(fromId);
            if (fromGate == null)
            {
                return new ValidationResult<ProgramLinkCreationComputedValues>($"Gate with ID {fromId} not found", ValidationErrorCode.FromGateNotFound);
            }

            var toGate = subroutineDefinition.GetGateById(toId);
            if (toGate == null)
            {
                return new ValidationResult<ProgramLinkCreationComputedValues>($"Gate with ID {toId} not found", ValidationErrorCode.ToGateNotFound);
            }

            IStatement fromStatement = fromGate as IStatement;
            if (fromStatement == null)
            {
                return new ValidationResult<ProgramLinkCreationComputedValues>($"Gate with ID {fromId} does not implement IStatement", ValidationErrorCode.FromGateNotStatement);
            }

            IStatement toStatement = toGate as IStatement;
            if (toStatement == null)
            {
                return new ValidationResult<ProgramLinkCreationComputedValues>($"Gate with ID {toId} does not implement IStatement", ValidationErrorCode.ToGateNotFound);
            }

            if (!fromStatement.OutwardPaths.Contains(flowName))
            {
                return new ValidationResult<ProgramLinkCreationComputedValues>($"Gate with ID {fromId} does not have outward path of '${flowName}'", ValidationErrorCode.OutwardPathNameNotFound);
            }

            return new ValidationResult<ProgramLinkCreationComputedValues>()
            {
                ComputedValues = new ProgramLinkCreationComputedValues()
                {
                    FromGate = fromGate,
                    ToGate = toGate,
                }
            };
        }
    }

    public class ProgramLinkCreationComputedValues
    {
        public IGate FromGate { get; set; }
        public IGate ToGate { get; set; }
    }
}
