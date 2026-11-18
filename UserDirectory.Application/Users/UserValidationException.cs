namespace UserDirectory.Application.Users;

public sealed class UserValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception("User validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
