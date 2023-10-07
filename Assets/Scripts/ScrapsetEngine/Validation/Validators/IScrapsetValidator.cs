namespace Scrapset.Engine
{
    public interface IScrapsetValidator<T>
    {
        public ValidationResult<T> Validate();
    }
}
