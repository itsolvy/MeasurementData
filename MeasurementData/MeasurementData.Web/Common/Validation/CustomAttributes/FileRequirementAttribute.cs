using System.ComponentModel.DataAnnotations;
using APRF.Web.FileStorage;

namespace APRF.Web.Common.Validation.CustomAttributes;

public class FileRequirementAttribute : BaseValidationAttribute
{
    public FileRequirementAttribute(string[]? fileExtensions = null)
    {
        this.FileExtensions = fileExtensions;
    }

    /// <summary>
    /// Расширения файлов через
    /// </summary>
    public string[]? FileExtensions { get; set; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as FileMetadata;

        if (file == null)
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Обьект не является типом {typeof(FileMetadata)}"
            );
        }
        if (this.FileExtensions != null && !this.FileExtensions.Contains(file.Extension))
        {
            return new ExtendedValidationResult(
                GetCheckedPropertyName(validationContext),
                $"Файлы должны иметь расширения {string.Join(", ", FileExtensions)}"
            );
        }
        return ValidationResult.Success!;
    }
}
