using System.Collections.Generic;

public interface IStatement
{
    IList<string> OutwardPaths { get; }
}
