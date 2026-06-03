namespace EduSync.SharedKernel.Results;

public sealed record Error(string Code, string Message, IReadOnlyDictionary<string, string[]>? FieldErrors = null)
{
    public static Error Validation(string message, IReadOnlyDictionary<string, string[]>? fieldErrors = null) =>
        new("VALIDATION_ERROR", message, fieldErrors);

    public static Error NotFound(string message) => new("NOT_FOUND", message);
    public static Error Forbidden(string message) => new("FORBIDDEN", message);
    public static Error Unauthorized(string message) => new("AUTH_FAILED", message);
    public static Error Conflict(string message) => new("CONFLICT", message);
}
