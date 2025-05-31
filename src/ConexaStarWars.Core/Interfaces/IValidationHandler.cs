namespace ConexaStarWars.Core.Interfaces;

public interface IValidationHandler<T>
{
    IValidationHandler<T>? NextHandler { get; set; }
    Task<ValidationResult> HandleAsync(T item);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? WarningMessage { get; set; }

    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }
}