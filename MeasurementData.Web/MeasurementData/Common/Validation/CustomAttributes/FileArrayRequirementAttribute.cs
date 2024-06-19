using System.ComponentModel.DataAnnotations;
using APRF.Web.FileStorage;

namespace APRF.Web.Common.Validation.CustomAttributes;

public class FileArrayRequirementAttribute : BaseValidationAttribute
{
    public FileArrayRequirementAttribute(
        string[]? fileExtensions = null,
        int maxElements = default,
        int maxMbSize = default
    )
    {
        FileExtensions = fileExtensions;
        MaxElements = maxElements;
        MaxMbSize = maxMbSize;
    }

    /// <summary>
    /// Расширения файлов через
    /// </summary>
    public string[]? FileExtensions { get; set; }

    public int MaxElements { get; set; }

    public int MaxMbSize { get; set; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var array = value as IList<FileMetadata>;

        if (array == null)
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Обьект не является массивом типа {typeof(FileMetadata)}"
            );
        }
        if (FileExtensions != null && !array.All(f => FileExtensions.Contains(f.Extension)))
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Файлы должны иметь расширения {string.Join(", ", FileExtensions)}"
            );
        }

        if (MaxElements != default && array.Count > MaxElements)
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Должно быть не более чем {MaxElements} файлов"
            );
        }

        if (MaxMbSize != default && array.Any(f => f.Size > (long)MaxMbSize * 1024 * 1024))
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Размер файла не должен превышать {MaxMbSize} Мб"
            );
        }
        return ValidationResult.Success!;
    }
}
