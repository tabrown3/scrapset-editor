using System.Collections.Generic;

public interface IStatement
{
    public List<string> OutwardPaths { get; set; }

    public void PerformSideEffect(Processor processor)
    {

    }
}
