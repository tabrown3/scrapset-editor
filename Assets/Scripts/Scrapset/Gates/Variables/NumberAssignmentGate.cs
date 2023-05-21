using System.Collections.Generic;
using UnityEngine;

public class NumberAssignmentGate : MonoBehaviour, IGate, IStatement
{
    public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

    public string Name => "Assignment";

    public string Description => "Assigns values to variables";

    public string Category => "Variables";

    // TODO: work out generics theme when convenient
    public Dictionary<string, ScrapsetTypes> InputParameters => new Dictionary<string, ScrapsetTypes>() { { "In", ScrapsetTypes.Number } };

    public Dictionary<string, ScrapsetTypes> OutputParameters => new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Number } };

    public int Id { get; set; }

    public void PerformSideEffect(Processor processor)
    {
        processor.AssignInputToOutput(this, "In", "Out");
        processor.Goto(this, "Next");
    }
}
