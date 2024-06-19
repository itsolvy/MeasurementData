using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = true
    )]
    public class CastStringValueToDateAttribute : BaseValidationAttribute
    {
        public CastStringValueToDateAttribute() { }

        protected override ValidationResult IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value is null)
            {
                return ValidationResult.Success!;
            }

            var checkedFieldInfo = GetCheckedFieldInfo<string>(validationContext, value);
            var str = checkedFieldInfo.Value ?? string.Empty;
            if (DateTime.TryParse(str, out _))
            {
                return ValidationResult.Success!;
            }

            return new ExtendedValidationResult(
                checkedFieldInfo.Name,
                $"Значение {str} не может быть преобразовано в дату."
            );
        }
    }
}
