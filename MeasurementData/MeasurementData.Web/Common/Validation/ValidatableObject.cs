using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation;

public abstract class ValidatableObject
{
    public virtual ValidationErrors Validate()
    {
        var validationResult = new List<ValidationResult>();
        var validationContext = new ValidationContext(this, null, null);
        Validator.TryValidateObject(this, validationContext, validationResult, true);

        var errorList = new ValidationErrors();
        foreach (var result in validationResult)
        {
            if (result.ErrorMessage == null)
            {
                continue;
            }

            string errorMessage = result.ErrorMessage;

            switch (result)
            {
                //TODO: тут ошибка. прогнать
                case ExtendedValidationResult extendedResult:
                    errorList.Add(extendedResult.ErrorsList);
                    break;

                default:
                    foreach (var fieldName in result.MemberNames)
                    {
                        errorList.Add(fieldName, result.ErrorMessage);
                    }
                    break;
            }
        }

        return errorList;
    }
}
