using Microsoft.EntityFrameworkCore;
using UserDirectory.Application.Users;
using UserDirectory.Domain.Entities;
using UserDirectory.Infrastructure.Persistence;

namespace UserDirectory.Infrastructure.Repositories;

public sealed class UserRepository(UserDirectoryDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Users.AsNoTracking().OrderBy(user => user.Name).ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AddAsync(user, cancellationToken);

    public void Remove(User user) => dbContext.Users.Remove(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
