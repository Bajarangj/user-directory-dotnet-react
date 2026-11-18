using UserDirectory.Application.Users.DTOs;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Users;

public sealed class UserService(IUserRepository repository) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.Age, request.City, request.State, request.Pincode);

        var user = new User(request.Name!, request.Age!.Value, request.City!, request.State!, request.Pincode!);
        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.Age, request.City, request.State, request.Pincode);

        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Update(request.Name!, request.Age!.Value, request.City!, request.State!, request.Pincode!);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        repository.Remove(user);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(string? name, int? age, string? city, string? state, string? pincode)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length is < 2 or > 100)
        {
            errors["name"] = ["Name is required and must be between 2 and 100 characters."];
        }

        if (age is null or < 0 or > 120)
        {
            errors["age"] = ["Age is required and must be between 0 and 120."];
        }

        AddRequiredError(errors, "city", city);
        AddRequiredError(errors, "state", state);

        if (string.IsNullOrWhiteSpace(pincode) || pincode.Trim().Length is < 4 or > 10)
        {
            errors["pincode"] = ["Pincode is required and must be between 4 and 10 characters."];
        }

        if (errors.Count > 0)
        {
            throw new UserValidationException(errors);
        }
    }

    private static void AddRequiredError(IDictionary<string, string[]> errors, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
        }
    }

    private static UserDto Map(User user) => new(user.Id, user.Name, user.Age, user.City, user.State, user.Pincode);
}
