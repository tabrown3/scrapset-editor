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

    [Test]
    public void SubroutineDefinition_RegisterGate_ShouldRegisterPreexistingGatesOfAnyLanguageFeature()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        // expression
        var addGate = new AddGate();
        var addGateId = subroutineDefinition.RegisterGate(addGate);
        Assert.AreEqual(addGateId, 1);

        // statement
        var assignmentGate = new AssignmentGate();
        var assignmentGateId = subroutineDefinition.RegisterGate(assignmentGate);
        Assert.AreEqual(assignmentGateId, 2);

        // variable
        var numberVariable = new NumberVariableGate();
        var numberVariableGateId = subroutineDefinition.RegisterGate(numberVariable);
        Assert.AreEqual(numberVariableGateId, 3);

        // another subroutine
        var innerSubroutineDefinition = new SubroutineDefinition("InnerTestDefinition");
        var subroutineGate = new SubroutineExpressionGate(innerSubroutineDefinition);
        var subroutineGateId = subroutineDefinition.RegisterGate(subroutineGate);
        Assert.AreEqual(subroutineGateId, 4);
    }

    [Test]
    public void SubroutineDefinition_GetGateById_ShouldReturnTheGateWithTheAssociatedIdOrNull()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var entrypoint = subroutineDefinition.GetGateById(0);
        Assert.IsInstanceOf<Entrypoint>(entrypoint);

        subroutineDefinition.RegisterGate(new AddGate());
        var addGate = subroutineDefinition.GetGateById(1);
        Assert.IsInstanceOf<AddGate>(addGate);

        subroutineDefinition.RegisterGate(new SubtractGate());
        var subtractGate = subroutineDefinition.GetGateById(2);
        Assert.IsInstanceOf<SubtractGate>(subtractGate);

        var nonExistentGate = subroutineDefinition.GetGateById(3);
        Assert.IsNull(nonExistentGate);
    }

    [Test]
    public void SubroutineDefinition_GetAllGates_ShouldReturnAllRegisteredGates()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");
        var allGates1 = subroutineDefinition.GetAllGates();
        Assert.AreEqual(1, allGates1.Count);
        Assert.NotNull(allGates1[0]);
        Assert.IsInstanceOf<Entrypoint>(allGates1[0]);

        subroutineDefinition.RegisterGate(new AddGate());
        var allGates2 = subroutineDefinition.GetAllGates();
        Assert.AreEqual(2, allGates2.Count);
        Assert.NotNull(allGates2[1]);
        Assert.IsInstanceOf<AddGate>(allGates2[1]);
    }

    [Test]
    public void SubroutineDefinition_CreateGate_ShouldCreateAndRegisterGates()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        // expression
        var addGateId = subroutineDefinition.CreateGate<AddGate>();
        Assert.AreEqual(addGateId, 1);

        // statement
        var assignmentGateId = subroutineDefinition.CreateGate<AssignmentGate>();
        Assert.AreEqual(assignmentGateId, 2);

        // variable
        var numberVariableGateId = subroutineDefinition.CreateGate<NumberVariableGate>();
        Assert.AreEqual(numberVariableGateId, 3);
    }

    [Test]
    public void SubroutineDefinition_RemoveGate_ShouldRemoveTheGateWithTheAssociatedId()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var addGate = new AddGate();
        subroutineDefinition.RegisterGate(addGate); // 1

        var assignmentGate = new AssignmentGate();
        subroutineDefinition.RegisterGate(assignmentGate); // 2

        var numberVariable = new NumberVariableGate();
        subroutineDefinition.RegisterGate(numberVariable); // 3

        var innerSubroutineDefinition = new SubroutineDefinition("InnerTestDefinition");
        var subroutineGate = new SubroutineExpressionGate(innerSubroutineDefinition);
        subroutineDefinition.RegisterGate(subroutineGate); // 4

        Assert.IsNotNull(subroutineDefinition.GetGateById(1));
        subroutineDefinition.RemoveGate(1);
        Assert.IsNull(subroutineDefinition.GetGateById(1));

        Assert.IsNotNull(subroutineDefinition.GetGateById(3));
        subroutineDefinition.RemoveGate(3);
        Assert.IsNull(subroutineDefinition.GetGateById(3));

        // cannot remove Entrypoint - currently it should throw
        Assert.Throws<System.Exception>(() => { subroutineDefinition.RemoveGate(0); });
    }
}
