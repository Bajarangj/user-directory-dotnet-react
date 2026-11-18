using Xunit;
using UserDirectory.Application.Users;
using UserDirectory.Application.Users.DTOs;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesAndReturnsUser()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository);

        var result = await service.CreateAsync(new CreateUserRequest("Jane Doe", 30, "Seattle", "WA", "98101"));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Single(repository.Users);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository());

        var exception = await Assert.ThrowsAsync<UserValidationException>(() =>
            service.CreateAsync(new CreateUserRequest("A", 121, "", "", "1")));

        Assert.Contains("name", exception.Errors.Keys);
        Assert.Contains("age", exception.Errors.Keys);
        Assert.Contains("city", exception.Errors.Keys);
        Assert.Contains("state", exception.Errors.Keys);
        Assert.Contains("pincode", exception.Errors.Keys);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingUser_UpdatesUser()
    {
        var user = new User("Jane Doe", 30, "Seattle", "WA", "98101");
        var repository = new FakeUserRepository(user);
        var service = new UserService(repository);

        var result = await service.UpdateAsync(user.Id, new UpdateUserRequest("John Doe", 40, "Austin", "TX", "78701"));

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(40, result.Age);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task DeleteAsync_WithMissingUser_ReturnsFalse()
    {
        var service = new UserService(new FakeUserRepository());

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    private sealed class FakeUserRepository(params User[] initialUsers) : IUserRepository
    {
        public List<User> Users { get; } = [.. initialUsers];

        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<User>>(Users);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public void Remove(User user) => Users.Remove(user);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
