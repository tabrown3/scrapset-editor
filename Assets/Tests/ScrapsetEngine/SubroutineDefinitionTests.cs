using NUnit.Framework;
using Scrapset.Engine;
using System.Linq;

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

        // cannot remove non-existent gate - currently it should throw
        Assert.Throws<System.Exception>(() => { subroutineDefinition.RemoveGate(5); });
    }

    // the following is a very light test of the CreateInputOutputLink behavior,
    //  since it's basically a pass-through for GateIORegistry
    [Test]
    public void SubroutineDefinition_CreateInputOutputLink_ShouldLinkOneInputToOneOutput()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var addGate = new AddGate();
        var addGateId = subroutineDefinition.RegisterGate(addGate);

        var assignmentGate = new AssignmentGate();
        var assignmentGateId = subroutineDefinition.RegisterGate(assignmentGate);

        var numberVariable = new NumberVariableGate();
        var numberVariableId = subroutineDefinition.RegisterGate(numberVariable);

        var actualHasInputLinks1 = subroutineDefinition.HasInputLinks(assignmentGateId);
        subroutineDefinition.CreateInputOutputLink(assignmentGateId, "In", addGateId, "Out");
        subroutineDefinition.CreateInputOutputLink(addGateId, "A", numberVariableId, "Out");
        subroutineDefinition.CreateInputOutputLink(addGateId, "B", numberVariableId, "Out");
        var actualHasInputLinks2 = subroutineDefinition.HasInputLinks(assignmentGateId);

        Assert.IsFalse(actualHasInputLinks1);
        Assert.IsTrue(actualHasInputLinks2);
    }

    // the following is a very light test of the CreateProgramFlowLink behavior,
    //  since it's basically a pass-through for ProgramFlowRegistry
    [Test]
    public void SubroutineDefinition_CreateProgramFlowLink_ShouldCreateLinkToAllowProgramToFlowFromOneStatementToTheNext()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var assignmentGate1 = new AssignmentGate();
        var assignmentGate1Id = subroutineDefinition.RegisterGate(assignmentGate1);

        var assignmentGate2 = new AssignmentGate();
        var assignmentGate2Id = subroutineDefinition.RegisterGate(assignmentGate2);

        var actualLink1 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");
        subroutineDefinition.CreateProgramFlowLink(assignmentGate1Id, "Next", assignmentGate2Id);
        var actualLink2 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");

        Assert.IsNull(actualLink1);
        Assert.IsNotNull(actualLink2);
    }

    // the following is a very light test of the RemoveProgramFlowLink behavior,
    //  since it's basically a pass-through for ProgramFlowRegistry
    [Test]
    public void SubroutineDefinition_RemoveProgramFlowLink_ShouldRemoveExistingProgramFlowLink()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var assignmentGate1 = new AssignmentGate();
        var assignmentGate1Id = subroutineDefinition.RegisterGate(assignmentGate1);

        var assignmentGate2 = new AssignmentGate();
        var assignmentGate2Id = subroutineDefinition.RegisterGate(assignmentGate2);

        subroutineDefinition.CreateProgramFlowLink(assignmentGate1Id, "Next", assignmentGate2Id);

        var actualLink1 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");
        subroutineDefinition.RemoveProgramFlowLink(assignmentGate1Id, "Next");
        var actualLink2 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");

        Assert.IsNotNull(actualLink1);
        Assert.IsNull(actualLink2);
    }

    // the following is a very light test of the RemoveAllProgramFlowLinks behavior,
    //  since it's basically a pass-through for ProgramFlowRegistry
    [Test]
    public void SubroutineDefinition_RemoveAllProgramFlowLinks_ShouldRemoveAllExistingProgramFlowLinks()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var assignmentGate1 = new AssignmentGate();
        var assignmentGate1Id = subroutineDefinition.RegisterGate(assignmentGate1);

        var assignmentGate2 = new AssignmentGate();
        var assignmentGate2Id = subroutineDefinition.RegisterGate(assignmentGate2);

        subroutineDefinition.CreateProgramFlowLink(assignmentGate1Id, "Next", assignmentGate2Id);
        subroutineDefinition.CreateProgramFlowLink(assignmentGate2Id, "Next", assignmentGate1Id);

        var actualLink1 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");
        var actualLink2 = subroutineDefinition.GetProgramFlowLink(assignmentGate2, "Next");
        subroutineDefinition.RemoveAllProgramFlowLinks(assignmentGate1Id);
        var actualLink3 = subroutineDefinition.GetProgramFlowLink(assignmentGate1, "Next");
        var actualLink4 = subroutineDefinition.GetProgramFlowLink(assignmentGate2, "Next");

        Assert.IsNotNull(actualLink1);
        Assert.IsNotNull(actualLink2);
        Assert.IsNull(actualLink3);
        Assert.IsNull(actualLink4);
    }

    [Test]
    public void SubroutineDefinition_DeclareLocalVariable_ShouldCreateNewVariableOfGivenType()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        var actual1 = subroutineDefinition.GetLocalVariableDeclaration("TestVariable");
        subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.Bool);
        var actual2 = subroutineDefinition.GetLocalVariableDeclaration("TestVariable");

        Assert.IsNull(actual1);
        Assert.IsNotNull(actual2);
        Assert.AreEqual(actual2.Name, "TestVariable");
        Assert.AreEqual(actual2.Type, ScrapsetTypes.Bool);
    }

    [Test]
    public void SubroutineDefinition_DeclareLocalVariable_ShouldThrowIfVariableNameIsAlreadyInUse()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.Bool);

        // redeclare variable with same name and same type - throw exception
        Assert.Throws<System.Exception>(() =>
        {
            subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.Bool);
        });

        // redeclare variable with same name and different type - throw exception
        Assert.Throws<System.Exception>(() =>
        {
            subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.Number);
        });
    }

    [Test]
    public void SubroutineDefinition_DeclareLocalVariable_ShouldAllowVariableWithDifferentCapitalization()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.Bool);

        // declare same name with different capitalization and same type
        Assert.DoesNotThrow(() =>
        {
            subroutineDefinition.DeclareLocalVariable("testVariable", ScrapsetTypes.Bool);
        });

        // declare same name with different capitalization and different type
        Assert.DoesNotThrow(() =>
        {
            subroutineDefinition.DeclareLocalVariable("testvariable", ScrapsetTypes.Number);
        });

        var declaration1 = subroutineDefinition.GetLocalVariableDeclaration("TestVariable");
        Assert.AreEqual(declaration1.Name, "TestVariable");
        Assert.AreEqual(declaration1.Type, ScrapsetTypes.Bool);

        var declaration2 = subroutineDefinition.GetLocalVariableDeclaration("testVariable");
        Assert.AreEqual(declaration2.Name, "testVariable");
        Assert.AreEqual(declaration2.Type, ScrapsetTypes.Bool);

        var declaration3 = subroutineDefinition.GetLocalVariableDeclaration("testvariable");
        Assert.AreEqual(declaration3.Name, "testvariable");
        Assert.AreEqual(declaration3.Type, ScrapsetTypes.Number);
    }

    [Test]
    public void SubroutineDefinition_CreateLocalVariableGate_ShouldCreateAGateThatActsAsALocalVariable()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        subroutineDefinition.DeclareLocalVariable("TestVariable", ScrapsetTypes.String);

        var variableGateId = subroutineDefinition.CreateLocalVariableGate<StringVariableGate>("TestVariable");

        var gate = subroutineDefinition.GetGateById(variableGateId);
        var variable = gate as IVariable;

        Assert.IsNotNull(gate);
        Assert.IsNotNull(variable);
        Assert.AreEqual("TestVariable", variable.Identifier);
        Assert.AreEqual(ScrapsetTypes.String, gate.InputParameters.First().Value.Type);
        Assert.AreEqual(ScrapsetTypes.String, gate.OutputParameters.First().Value.Type);
    }

    [Test]
    public void SubroutineDefinition_CreateLocalVariableGate_ShouldThrowIfVariableIsNotDeclared()
    {
        var subroutineDefinition = new SubroutineDefinition("TestDefinition");

        Assert.Throws<System.Exception>(() =>
        {
            var variableGateId = subroutineDefinition.CreateLocalVariableGate<StringVariableGate>("TestVariable");
        });
    }
}
