﻿using System.Collections.Generic;

namespace Scrapset.Engine
{
    public class ValidationResult
    {
        public ValidationResult() { }
        public ValidationResult(string message, ValidationErrorCode errorCode)
        {
            AddError(message, errorCode);
        }
        public ValidationResult(string message, ValidationWarningCode warningCode)
        {
            AddWarning(message, warningCode);
        }

        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;

        public List<ValidationError> Errors = new List<ValidationError>();
        public List<ValidationWarning> Warnings = new List<ValidationWarning>();

        public void AddError(string message, ValidationErrorCode errorCode)
        {
            Errors.Add(new ValidationError(message, errorCode));
        }

        public void AddWarning(string message, ValidationWarningCode warningCode)
        {
            Warnings.Add(new ValidationWarning(message, warningCode));
        }
    }
}