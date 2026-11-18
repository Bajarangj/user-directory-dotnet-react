namespace UserDirectory.Application.Users.DTOs;

public sealed record CreateUserRequest(
    string? Name,
    int? Age,
    string? City,
    string? State,
    string? Pincode);
