namespace UserDirectory.Application.Users.DTOs;

public sealed record UpdateUserRequest(
    string? Name,
    int? Age,
    string? City,
    string? State,
    string? Pincode);
