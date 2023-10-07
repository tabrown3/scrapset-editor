namespace Scrapset.Engine
{
    public enum ValidationErrorCode
    {
        Unknown
    }

    public class ValidationError
    {
        public string Message { get; set; }
        public ValidationErrorCode ErrorCode { get; set; }

        public ValidationError(string message, ValidationErrorCode errorCode)
        {
            Message = message;
            ErrorCode = errorCode;
        }
    }
}
