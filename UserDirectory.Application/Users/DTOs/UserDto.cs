namespace UserDirectory.Application.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Name,
    int Age,
    string City,
    string State,
    string Pincode);
