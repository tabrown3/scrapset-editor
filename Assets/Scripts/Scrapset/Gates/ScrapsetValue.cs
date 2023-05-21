public class ScrapsetValue
{
    public ScrapsetTypes Type { get; }
    public object Value { get; set; } // TODO: check to make sure the dynamic value passed in is of ScrapsetType type

    public ScrapsetValue(ScrapsetTypes type)
    {
        Type = type;
    }
}
