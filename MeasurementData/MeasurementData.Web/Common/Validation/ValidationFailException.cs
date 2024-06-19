using System.Runtime.Serialization;

namespace APRF.Web.Common.Validation;

/// <summary>
/// Ошибка валидации
/// </summary>
[Serializable]
public class ValidationFailException : Exception
{
    public ValidationErrors ErrorsList { get; }

    public ValidationFailException(ValidationErrors errors)
        : base("Validation exception")
    {
        ErrorsList = errors;
    }

    protected ValidationFailException(
        SerializationInfo serializationInfo,
        StreamingContext streamingContext
    )
        : base(serializationInfo, streamingContext)
    {
        ErrorsList ??= new ValidationErrors();
    }
}
