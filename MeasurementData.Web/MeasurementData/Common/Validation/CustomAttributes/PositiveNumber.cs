using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

/// <summary>
/// Specified that a data field value must be less than specified one.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = true
)]
public sealed class PositiveNumber : BaseValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        // TODO: сделать, чтобы работало со всеми числовыми типами
        var checkedFieldInfo = GetCheckedFieldInfo<decimal>(validationContext, value);
        var currentLongValue = checkedFieldInfo.Value;

        if (currentLongValue >= 0)
        {
            return ValidationResult.Success!;
        }

        var errorMessage =
            ErrorMessage
            ?? ($"Значение поля \"{checkedFieldInfo.DisplayName}\" не должно быть отрицательным");

        return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
    }
}
