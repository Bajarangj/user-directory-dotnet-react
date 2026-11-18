using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserDirectory.Application.Users;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Repositories;

namespace UserDirectory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=/data/app.db";
        const string dataSourcePrefix = "Data Source=";
        var dataSource = connectionString.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase)
            ? connectionString[dataSourcePrefix.Length..].Trim()
            : null;

        if (!string.IsNullOrWhiteSpace(dataSource) && !dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        services.AddDbContext<UserDirectoryDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
