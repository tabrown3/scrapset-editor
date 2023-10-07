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
                throw new System.Exception($"Gate with ID {fromId} not found");
            }

            var toGate = subroutineDefinition.GetGateById(toId);
            if (toGate == null)
            {
                throw new System.Exception($"Gate with ID {toId} not found");
            }

            IStatement fromStatement = fromGate as IStatement;
            if (fromStatement == null)
            {
                throw new System.Exception($"Gate with ID {fromId} does not implement IStatement");
            }

            IStatement toStatement = toGate as IStatement;
            if (toStatement == null)
            {
                throw new System.Exception($"Gate with ID {toId} does not implement IStatement");
            }

            if (!fromStatement.OutwardPaths.Contains(flowName))
            {
                throw new System.Exception($"Gate with ID {fromId} does not have outward path of '${flowName}'");
            }

            throw new System.NotImplementedException();
        }
    }
}
