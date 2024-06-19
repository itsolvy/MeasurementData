using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

public class StringLengthWithMessageAttribute : BaseValidationAttribute
{
    public int MaximumLength { get; }
    public int MinimumLength { get; set; }

    public StringLengthWithMessageAttribute(int maximumLength)
    {
        MaximumLength = maximumLength;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (MinimumLength > MaximumLength)
        {
            throw new ArgumentException("MaximumLength must be greater than MinimumLength");
        }

        if (value is null)
        {
            return ValidationResult.Success!;
        }

        var checkedFieldInfo = GetCheckedFieldInfo<string>(validationContext, value);
        var str = checkedFieldInfo.Value?.Trim() ?? string.Empty;

        if (str.Length >= MinimumLength && str.Length <= MaximumLength)
        {
            return ValidationResult.Success!;
        }

        var errorMessage =
            ErrorMessage
            ?? (
                MinimumLength == 0
                    ? $"Длина должна быть не более {MaximumLength} символов"
                    : $"Длина должна быть от {MinimumLength} до {MaximumLength} символов"
            );

        return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
    }
}
