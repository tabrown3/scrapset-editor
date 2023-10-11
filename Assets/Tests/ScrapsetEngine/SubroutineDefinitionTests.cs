using NUnit.Framework;
using Scrapset.Engine;

public class SubroutineDefinitionTests
{
    [Test]
    public void SubroutineDefinition_Constructor_ShouldPopulateProperties()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");
        Assert.AreEqual(subroutineDefinition.Identifier, "TestDefinition");
        Assert.AreEqual(subroutineDefinition.EntrypointId, 0);
        Assert.NotNull(subroutineDefinition.InputParameters);
        Assert.NotNull(subroutineDefinition.OutputParameters);
        Assert.NotNull(subroutineDefinition.GenericTypeReconciler);

        var gates = subroutineDefinition.GetAllGates();
        Assert.AreEqual(gates.Count, 1);

        var entrypoint = gates[0];
        Assert.IsInstanceOf<Entrypoint>(entrypoint);
    }
}
