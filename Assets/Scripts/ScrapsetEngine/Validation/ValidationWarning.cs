namespace Scrapset.Engine
{
    public enum ValidationWarningCode
    {
        Unknown
    }

    public class ValidationWarning
    {
        public string Message { get; set; }
        public ValidationWarningCode WarningCode { get; set; }

        public ValidationWarning(string message, ValidationWarningCode warningCode)
        {
            Message = message;
            WarningCode = warningCode;
        }
    }
}
