using System.Collections.Generic;
using UnityEngine;

public class AssignmentGate : MonoBehaviour, IGate, IStatement
{
    public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

    public string Name => "Assignment";

    public string Description => "Assigns values to variables";

    public string Category => "Variables";

    public Dictionary<string, ScrapsetTypes> Inputs => new Dictionary<string, ScrapsetTypes>() { { "In", ScrapsetTypes.Generic } };

    public Dictionary<string, ScrapsetTypes> Outputs => new Dictionary<string, ScrapsetTypes>() { { "Out", ScrapsetTypes.Generic } };

    public int Id { get; set; }

    public void PerformSideEffect(Processor processor)
    {
        processor.AssignInputToOutput(this, "In", "Out");
        processor.Goto(this, "Next");
    }
}
