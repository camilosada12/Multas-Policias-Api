using Entity.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

public static class DatabaseService
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["MigrationProvider"]?.ToLowerInvariant();
        var sql = config.GetConnectionString("SqlServer");
        var pg = config.GetConnectionString("Postgres");
        var my = config.GetConnectionString("MySql");

        switch (provider)
        {
            case "sqlserver":
                if (!string.IsNullOrWhiteSpace(sql))
                    services.AddDbContext<ApplicationDbContext>(opt =>
                        opt.UseSqlServer(sql, s =>
                        {
                            s.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                            s.EnableRetryOnFailure();
                        }));
                break;

            case "postgres":
                if (!string.IsNullOrWhiteSpace(pg))
                    services.AddDbContext<PostgresDbContext>(opt =>
                        opt.UseNpgsql(pg, n =>
                        {
                            n.MigrationsAssembly(typeof(PostgresDbContext).Assembly.FullName);
                            n.EnableRetryOnFailure();
                        }));
                break;

            case "mysql":
                if (!string.IsNullOrWhiteSpace(my))
                    services.AddDbContext<MySqlApplicationDbContext>(opt =>
                        opt.UseMySql(my, ServerVersion.AutoDetect(my)));
                break;

            default:
                throw new InvalidOperationException($"Proveedor de base de datos no válido: {provider}");
        }

        return services;
    }
}
