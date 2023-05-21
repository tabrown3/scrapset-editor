public interface IVariable
{
    public ScrapsetValue Read();

    public void Write(ScrapsetValue value);
}
