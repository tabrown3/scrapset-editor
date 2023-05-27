public interface IGate : IInputOutput
{
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }
    public int Id { get; set; }
}
