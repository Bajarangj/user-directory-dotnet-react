using Microsoft.AspNetCore.Mvc;
using UserDirectory.API.Controllers;
using UserDirectory.Application.Users;
using UserDirectory.Application.Users.DTOs;
using Xunit;

namespace UserDirectory.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithUsers()
    {
        var expected = new UserDto(Guid.NewGuid(), "Jane Doe", 30, "Seattle", "WA", "98101");
        var controller = new UsersController(new FakeUserService([expected]));

        var result = await controller.GetAll(CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IReadOnlyList<UserDto>>(response.Value);
        Assert.Single(users);
        Assert.Equal(expected.Id, users[0].Id);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var controller = new UsersController(new FakeUserService());

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreatedAtAction()
    {
        var created = new UserDto(Guid.NewGuid(), "Jane Doe", 30, "Seattle", "WA", "98101");
        var controller = new UsersController(new FakeUserService { Created = created });

        var result = await controller.Create(
            new CreateUserRequest("Jane Doe", 30, "Seattle", "WA", "98101"),
            CancellationToken.None);

        var response = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.GetById), response.ActionName);
        Assert.Equal(created.Id, ((UserDto)response.Value!).Id);
    }

    [Fact]
    public async Task Create_WhenInvalid_ReturnsBadRequest()
    {
        var controller = new UsersController(new FakeUserService
        {
            ValidationException = new UserValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Name is required."]
            })
        });

        var result = await controller.Create(new CreateUserRequest(null, null, null, null, null), CancellationToken.None);

        var response = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(response.Value);
    }

    private sealed class FakeUserService(IReadOnlyList<UserDto>? users = null) : IUserService
    {
        private readonly IReadOnlyList<UserDto> users = users ?? [];

        public UserDto? Created { get; init; }

        public UserValidationException? ValidationException { get; init; }

        public Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(users);

        public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(users.SingleOrDefault(user => user.Id == id));

        public Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (ValidationException is not null)
            {
                throw ValidationException;
            }

            return Task.FromResult(Created!);
        }

        public Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult<UserDto?>(null);

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
