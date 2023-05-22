public interface IVariable
{
    public ScrapsetValue Reference { get; set; }
    public string VariableName { get; set; }
    public ScrapsetValue Read();

    public void Write(ScrapsetValue value);
}
