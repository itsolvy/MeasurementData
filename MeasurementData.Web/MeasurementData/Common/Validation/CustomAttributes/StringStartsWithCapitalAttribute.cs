using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

public class StringStartsWithCapitalAttribute : BaseValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        var checkedFieldInfo = GetCheckedFieldInfo<string>(validationContext, value);
        var str = checkedFieldInfo.Value;

        if (string.IsNullOrEmpty(str))
        {
            return ValidationResult.Success!;
        }

        if (char.IsUpper(str[0]))
        {
            return ValidationResult.Success!;
        }

        var errorMessage = "Значение должно начинаться с заглавной буквы";
        return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
    }
}
