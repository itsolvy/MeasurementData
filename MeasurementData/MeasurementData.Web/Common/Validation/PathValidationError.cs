namespace APRF.Web.Common.Validation;

public class PathValidationError
{
    public PathValidationError(object[] path, string[] errors)
    {
        this.Path = path ?? throw new NotImplementedException(nameof(path));
        this.Errors = errors ?? throw new NotImplementedException(nameof(errors));
    }

    [ArrayOfOpenApi3Types("string", "number")]
    public object[] Path { get; set; }

    public string[] Errors { get; set; }
}
