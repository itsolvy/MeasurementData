using System.Collections;
using Newtonsoft.Json.Serialization;

namespace MeasurementData.Web.Common;

/// <summary>
/// Плоский список ошибок
/// </summary>
public class ValidationErrors : IReadOnlyCollection<KeyValuePair<object[], string[]>>
{
    private static readonly IEqualityComparer<object[]> _objArrayComparer = new ObjArrayComparer();
    private readonly Dictionary<object[], string[]> _dictionary = new(_objArrayComparer);

    /// <summary>
    /// Есть ошибки
    /// </summary>
    public bool HasErrors => _dictionary.Count > 0;

    /// <summary>
    /// Добавить ошибку
    /// </summary>
    /// <param name="key">Составной ключ</param>
    /// <param name="errors">Массив ошибок</param>
    public void Add(object[] key, string[] errors)
    {
        if (key.Length == 0)
        {
            throw new ArgumentException(nameof(key));
        }

        if (errors.Length == 0)
        {
            throw new ArgumentException(nameof(errors));
        }

        if (_dictionary.TryGetValue(key, out var value))
        {
            _dictionary[key] = value.Concat(errors).Distinct().ToArray();
        }
        else
        {
            _dictionary[key] = errors;
        }
    }

    /// <summary>
    /// Добавить ошибку
    /// </summary>
    /// <param name="key">Составной ключ</param>
    /// <param name="error">Ошибка</param>
    public void Add(object[] key, string error)
    {
        this.Add(key, new[] { error });
    }

    /// <summary>
    /// Добавить ошибку
    /// </summary>
    /// <param name="key">Составной ключ</param>
    /// <param name="error">Ошибка</param>
    public void Add(object key, string error)
    {
        this.Add(new[] { key }, error);
    }

    /// <summary>
    /// Добавить ошибку
    /// </summary>
    /// <param name="parent">Составной ключ родительского обьекта</param>
    /// <param name="key">Ключ</param>
    /// <param name="error">Ошибка</param>
    public void Add(object[] parent, object key, string error)
    {
        this.Add(parent.AsEnumerable().Concat(new[] { key }).ToArray(), error);
    }

    /// <summary>
    /// Добавить ошибки валидации из другого списка
    /// </summary>
    public void Add(ValidationErrors keyValues)
    {
        foreach (var error in keyValues)
        {
            Add(error.Key, error.Value);
        }
    }

    /// <summary>
    /// Добавить ошибки валидации из списка ошибок дочернего обьекта
    /// </summary>
    /// <param name="key">Составной ключ родительского обьекта</param>
    /// <param name="errorsList">Обьект валидации</param>
    public void AddChild(object[] key, ValidationErrors errorsList)
    {
        foreach (var error in errorsList)
        {
            Add(key.Concat(error.Key).ToArray(), error.Value);
        }
    }

    /// <summary>
    /// Сериализовать обьект ошибок к плоскому списку для frontend
    /// </summary>
    public PathValidationError[] MapToPathErrorList(bool camelCasePropertyNames)
    {
        var camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
        return _dictionary
            .Select(pair =>
            {
                var key = pair.Key
                    .Select(keyPart =>
                    {
                        if (keyPart is string)
                        {
                            return camelCasePropertyNames
                                ? camelCaseContractResolver.GetResolvedPropertyName(
                                    (keyPart as string)!
                                )
                                : keyPart;
                        }

                        return keyPart;
                    })
                    .ToArray();
                return new PathValidationError(key, pair.Value);
            })
            .ToArray();
    }

    #region IReadOnlyCollection
    public int Count => _dictionary.Count;

    public IEnumerator<KeyValuePair<object[], string[]>> GetEnumerator() =>
        _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    #endregion

    /// <summary>
    /// Класс для сравнения наборов ключенй
    /// </summary>
    private sealed class ObjArrayComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[]? x, object[]? y) =>
            StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        public int GetHashCode(object[] obj) =>
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}
