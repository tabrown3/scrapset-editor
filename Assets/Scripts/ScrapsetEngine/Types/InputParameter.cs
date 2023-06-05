namespace Scrapset.Engine
{
    public class InputParameter : Parameter
    {
        public bool IsDeferred { get; set; } // if true, the input will be present in the deferred input value dict
    }
}
