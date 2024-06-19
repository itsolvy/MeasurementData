using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace APRF.Web.Common.Validation.CustomAttributes
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = true
    )]
    public class CastStringValueToNumbersAttribute : BaseValidationAttribute
    {
        private readonly Type _typeOfNumber;

        public CastStringValueToNumbersAttribute(Type typeOfNumber)
        {
            _typeOfNumber = typeOfNumber;
        }

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
            bool? result = _typeOfNumber switch
            {
                { } when _typeOfNumber.FullName == typeof(decimal).FullName
                    => decimal.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(byte).FullName
                    => byte.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(short).FullName
                    => short.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(int).FullName => int.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(long).FullName
                    => long.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(sbyte).FullName
                    => sbyte.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(ushort).FullName
                    => ushort.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(uint).FullName
                    => uint.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(ulong).FullName
                    => ulong.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(float).FullName
                    => float.TryParse(str, out _),
                { } when _typeOfNumber.FullName == typeof(BigInteger).FullName
                    => BigInteger.TryParse(str, out _),
                _ => null
            };

            if (result is null)
            {
                return new ExtendedValidationResult(
                    checkedFieldInfo.Name,
                    $"Тип {_typeOfNumber!.Name} не является числом"
                );
            }

            if (result.Value)
            {
                return ValidationResult.Success!;
            }

            return new ExtendedValidationResult(
                checkedFieldInfo.Name,
                $"Значение {str} не может быть преобразовано в число типа {_typeOfNumber!.Name}."
            );
        }
    }
}
