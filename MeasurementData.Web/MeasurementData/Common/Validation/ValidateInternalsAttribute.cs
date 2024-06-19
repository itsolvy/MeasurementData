using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation;

/// <summary>
/// Атрибут валидации значений свойств, полей или параметров.
/// Указывает, что значение не должно быть `NULL`, пустым (в случае строк/массивов/списков) или равняться значению по умолчанию.
/// </summary>
public sealed class ValidateInternalsAttribute : BaseValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var checkedFieldInfo = GetCheckedFieldInfo<object?>(validationContext, value);

        var errorMessage = ErrorMessage ?? "Ошибка валидации внутри объекта";

        switch (checkedFieldInfo.Value)
        {
            case ValidatableObject validatableObject:
                var validationResult = validatableObject.Validate();

                return validationResult.HasErrors
                    ? new ExtendedValidationResult(
                        checkedFieldInfo.Name,
                        errorMessage,
                        validationResult
                    )
                    : ValidationResult.Success!;

            case IEnumerable<ValidatableObject> collection:
                return ValidateCollection(collection, checkedFieldInfo.Name);

            case null:
                return ValidationResult.Success!;

            default:
                throw new InvalidOperationException(
                    "Can validate `ValidatableObject` or `IEnumerable<ValidatableObject>` only!"
                );
        }
    }

    private static ValidationResult ValidateCollection(
        IEnumerable<ValidatableObject> collection,
        string fieldName
    )
    {
        var validatableObjects = collection as ValidatableObject[] ?? collection.ToArray();

        if (!validatableObjects.Any())
        {
            return ValidationResult.Success!;
        }
        var i = 0;

        var errors = new ValidationErrors();

        foreach (var validatableObject in validatableObjects)
        {
            var validationResult = validatableObject.Validate();

            if (validationResult.HasErrors)
            {
                errors.AddChild(new object[] { fieldName, i }, validationResult);
            }

            i++;
        }

        return errors.HasErrors
            ? new ExtendedValidationResult(fieldName, "Ошибка валидации внутри коллекции", errors)
            : ValidationResult.Success!;
    }
}
