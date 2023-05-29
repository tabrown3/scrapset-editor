namespace Scrapset.Engine
{
    public interface IGate : IInputOutput
    {
        public string Name { get; }
        public string Description { get; }
        public LanguageCategory Category { get; set; }
        public int Id { get; set; }
    }
}
