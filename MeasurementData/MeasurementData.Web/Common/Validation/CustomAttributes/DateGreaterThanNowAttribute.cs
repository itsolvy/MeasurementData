using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

/// <summary>
/// Specified that a data field value must be greater than <see cref="DateTime.Now"/>.
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = true
)]
public sealed class DateGreaterThanNowAttribute : BaseValidationAttribute
{
    private readonly bool _allowEqualDateTimes;

    public DateGreaterThanNowAttribute(bool allowEqualDateTimes = true)
    {
        _allowEqualDateTimes = allowEqualDateTimes;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        var checkedFieldInfo = GetCheckedFieldInfo<DateTime?>(validationContext, value);

        var currentDate = checkedFieldInfo.Value;
        var compareDate = DateTime.UtcNow.Date;

        if (currentDate > compareDate)
        {
            return ValidationResult.Success!;
        }

        if (compareDate == currentDate && _allowEqualDateTimes)
        {
            return ValidationResult.Success!;
        }

        var errorMessage =
            ErrorMessage
            ?? $"Дата и время \"{checkedFieldInfo.DisplayName}\" не может быть меньше текущей даты";

        return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
    }
}
