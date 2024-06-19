using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace APRF.Web.Common.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public abstract class BaseValidationAttribute : ValidationAttribute
{
    protected FieldInfo<T> GetCheckedFieldInfo<T>(ValidationContext context, object? value)
    {
        var t = typeof(T);
        if (t.IsValueType && Nullable.GetUnderlyingType(t) is null && value is null)
        {
            throw new InvalidOperationException(
                $"Can not use null with not nullable generic type {t.Name}"
            );
        }

        var name = GetCheckedPropertyName(context);
        var propertyInfo = GetPropertyInfo(context, name);
        var displayName = GetDisplayName(propertyInfo, name);
        if (value != null)
        {
            var fieldValue = (T)value;
            return new FieldInfo<T>(name, displayName, fieldValue, propertyInfo);
        }

        return new FieldInfo<T>(name, displayName, default, propertyInfo);
    }

    protected FieldInfo<T?> GetFieldInfo<T>(ValidationContext context, string propertyName)
    {
        var propertyInfo = GetPropertyInfo(context, propertyName);
        var fieldValue = GetPropertyValue<T>(context, propertyInfo);

        var displayName = GetDisplayName(propertyInfo, propertyName);

        return new FieldInfo<T?>(propertyName, displayName, fieldValue, propertyInfo);
    }

    public static string GetCheckedPropertyName(ValidationContext context)
    {
        var propertyName = context.MemberName;

        if (propertyName == null)
        {
            throw new InvalidOperationException(
                "Validator error: cannot get the checked property name."
            );
        }

        return propertyName;
    }

    private static T? GetPropertyValue<T>(ValidationContext context, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(context.ObjectInstance, null);
        if (value is null)
        {
            return default;
        }

        return (T)value;
    }

    private static string GetDisplayName(ICustomAttributeProvider propertyInfo, string propertyName)
    {
        var currentDisplayNameAttributes = (DisplayNameAttribute[]?)
            propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false);
        var currentDisplayName = currentDisplayNameAttributes?.FirstOrDefault()?.DisplayName;
        return currentDisplayName ?? propertyName;
    }

    private static PropertyInfo GetPropertyInfo(ValidationContext context, string propertyName)
    {
        return GetPropertyInfo(GetObjectType(context), propertyName);
    }

    private static PropertyInfo GetPropertyInfo(Type validationType, string propertyName)
    {
        var propertyInfo = validationType.GetProperty(propertyName);

        if (propertyInfo == null)
        {
            throw new InvalidOperationException(
                $"Validator error: cannot get the '{propertyName}' property info."
            );
        }

        return propertyInfo;
    }

    private static Type GetObjectType(ValidationContext context)
    {
        var objectType = context.ObjectType;

        if (objectType == null)
        {
            throw new InvalidOperationException(
                "Validator error: the validation object type not found"
            );
        }

        return objectType;
    }
}

public class FieldInfo<T>
{
    public string Name { get; }
    public string DisplayName { get; }
    public PropertyInfo PropertyInfo { get; }
    public Type PropertyType { get; }
    public T? Value { get; }

    public FieldInfo(string name, string displayName, T? value, PropertyInfo propertyInfo)
    {
        DisplayName = displayName;
        Name = name;
        Value = value;
        PropertyInfo = propertyInfo;
        PropertyType = propertyInfo.PropertyType;
    }
}
