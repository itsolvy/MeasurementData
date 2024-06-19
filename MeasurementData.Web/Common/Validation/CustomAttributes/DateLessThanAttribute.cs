using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

/// <summary>
/// Specified that a data field value must be less than specified one.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = true
)]
public sealed class DateLessThanAttribute : BaseValidationAttribute
{
    private readonly string _propertyNameToCompare;
    private readonly bool _allowEqualDateTimes;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateLessThanAttribute"/> class.
    /// </summary>
    /// <param name="propertyNameToCompare">
    /// The property name to compare.
    /// </param>
    /// <param name="allowEqualDateTimes">
    /// Specifies that the comparable values can be equal.
    /// </param>
    public DateLessThanAttribute(string propertyNameToCompare, bool allowEqualDateTimes = false)
    {
        _propertyNameToCompare = propertyNameToCompare;
        _allowEqualDateTimes = allowEqualDateTimes;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var checkedFieldInfo = GetCheckedFieldInfo<DateTime?>(validationContext, value);
        var compareFieldInfo = GetFieldInfo<DateTime?>(validationContext, _propertyNameToCompare);

        var currentDate = checkedFieldInfo.Value;
        var compareDate = compareFieldInfo.Value;

        if (!(compareDate.HasValue && currentDate.HasValue))
        {
            return ValidationResult.Success!;
        }

        if (compareDate > currentDate)
        {
            return ValidationResult.Success!;
        }

        if (compareDate == currentDate && _allowEqualDateTimes)
        {
            return ValidationResult.Success!;
        }

        var errorMessage =
            ErrorMessage
            ?? (
                _allowEqualDateTimes
                    ? $"Значение поля \"{checkedFieldInfo.DisplayName}\" должно быть меньше или равно значению поля \"{compareFieldInfo.DisplayName}\""
                    : $"Значение поля \"{checkedFieldInfo.DisplayName}\" должно быть меньше значения поля \"{compareFieldInfo.DisplayName}\""
            );

        return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
    }
}
