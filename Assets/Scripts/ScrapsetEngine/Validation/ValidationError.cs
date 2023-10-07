namespace Scrapset.Engine
{
    public enum ValidationErrorCode
    {
        Unknown,
        /* Gate IO link creation codes */
        OutputGateNotFound,
        InputGateNotFound,
        OutputParamNotFound,
        InputParamNotFound,
        GenericToGenericUnsupported, // generic-to-generic links will hopefully be supported at some point
        GenericInputTypeIdentifierNotFound,
        GenericOutputTypeIdentifierNotFound,
        TypeMismatch,
        InputParamAlreadyLinked,
        /* Program flow link creation codes */
        FromGateNotFound,
        ToGateNotFound,
        FromGateNotStatement,
        ToGateNotStatement,
        OutwardPathNameNotFound,
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
