using System.ComponentModel.DataAnnotations;

namespace APRF.Web.Common.Validation.CustomAttributes;

// TODO: мб унаследоваться от BaseValidationAttribute, но в стандартном RangeAttribute много логики, не хотелось бы ее переписывать
public class RangeWithMessageAttribute : RangeAttribute
{
    public RangeWithMessageAttribute(double minimum, double maximum)
        : base(minimum, maximum)
    {
        ErrorMessage = GetErrorMessage(minimum, maximum);
    }

    public RangeWithMessageAttribute(int minimum, int maximum)
        : base(minimum, maximum)
    {
        ErrorMessage = GetErrorMessage(minimum, maximum);
    }

    public RangeWithMessageAttribute(Type type, string minimum, string maximum)
        : base(type, minimum, maximum)
    {
        ErrorMessage = GetErrorMessage(minimum, maximum);
    }

    private static string GetErrorMessage<T>(T minimum, T maximum)
    {
        return $"Значение должно быть от {minimum} до {maximum}";
    }
}
