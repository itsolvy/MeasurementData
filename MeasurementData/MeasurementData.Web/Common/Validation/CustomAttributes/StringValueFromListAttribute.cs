using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes
{
    public class StringValueFromListAttribute : BaseValidationAttribute
    {
        private readonly string[] _listStrings;

        public StringValueFromListAttribute(string[] listStrings)
        {
            _listStrings = listStrings;
        }

        protected override ValidationResult IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (_listStrings.Length == 0)
            {
                throw new ArgumentException("The list of valid string values is not defined");
            }

            if (value is null)
            {
                return ValidationResult.Success!;
            }

            var checkedFieldInfo = GetCheckedFieldInfo<string>(validationContext, value);
            var passedString = checkedFieldInfo.Value?.Trim();

            if (_listStrings.Contains(passedString))
            {
                return ValidationResult.Success!;
            }

            var errorMessage =
                ErrorMessage ?? "Значение поля не входит в список допустимых значений";
            return new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);
        }
    }
}
