using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation;

/// <summary>
/// Атрибут валидации значений свойств, полей или параметров.
/// Указывает, что значение не должно быть `NULL`, пустым (в случае строк/массивов/списков) или равняться значению по умолчанию.
/// </summary>
public sealed class NotNullEmptyDefaultAttribute : BaseValidationAttribute
{
    public const string DEFAULT_ERROR_MESSAGE = "Значение обязательно для заполнения";

    /// <inheritdoc />
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var checkedFieldInfo = GetCheckedFieldInfo<object?>(validationContext, value);
        var checkedFieldType = checkedFieldInfo.PropertyType;

        var errorMessage = ErrorMessage ?? DEFAULT_ERROR_MESSAGE;
        var validationError = new ExtendedValidationResult(checkedFieldInfo.Name, errorMessage);

        switch (value)
        {
            case null:
                return validationError;

            case ICollection collection:
                if (collection.Count == 0)
                {
                    return validationError;
                }
                break;

            case string str:
                if (string.IsNullOrWhiteSpace(str))
                {
                    return validationError;
                }
                break;

            default:

                {
                    object? defaultValue = null;

                    if (checkedFieldType.IsValueType)
                    {
                        defaultValue = Activator.CreateInstance(checkedFieldType);
                    }

                    if (
                        checkedFieldType != typeof(bool)
                        && defaultValue != null
                        && defaultValue.Equals(value)
                    )
                    {
                        return validationError;
                    }
                }
                break;
        }

        return ValidationResult.Success!;
    }
}
