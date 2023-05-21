using System.Collections.Generic;

public interface IStatement : IExecutable
{
    public List<string> OutwardPaths { get; set; }
}
