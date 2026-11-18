using UserDirectory.Domain.Entities;

namespace UserDirectory.Application.Users;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Remove(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
