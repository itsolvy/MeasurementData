using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation;

public class ExtendedValidationResult : ValidationResult
{
    public ValidationErrors ErrorsList { get; }

    public ExtendedValidationResult(string fieldName, string errorMessage)
        : base(errorMessage, new[] { fieldName })
    {
        ErrorsList = new ValidationErrors();
        ErrorsList.Add(fieldName, errorMessage);
    }

    public ExtendedValidationResult(string fieldName, string errorMessage, ValidationErrors errors)
        : base(errorMessage, new[] { fieldName })
    {
        ErrorsList = errors;
    }
}
