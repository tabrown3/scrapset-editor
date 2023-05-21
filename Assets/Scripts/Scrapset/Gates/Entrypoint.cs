using System.Collections.Generic;
using UnityEngine;

public class Entrypoint : MonoBehaviour, IGate, IStatement
{
    public List<string> OutwardPaths { get; set; } = new List<string>() { "Next" };

    public string Name => "Entrypoint";

    public string Description => "The program's starting point";

    public string Category => "System";

    public Dictionary<string, ScrapsetTypes> Inputs => new Dictionary<string, ScrapsetTypes>();

    public Dictionary<string, ScrapsetTypes> Outputs => new Dictionary<string, ScrapsetTypes>();

    public int Id { get; set; }

    public void PerformSideEffect(Processor processor)
    {
        processor.Goto(this, "Next");
    }
}
