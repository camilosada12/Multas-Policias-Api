using Entity.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // 👈 importante

namespace Web.Service
{
    public static class DatabaseService
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            var pg = config.GetConnectionString("Postgres");

            // Registrar AuditManager si lo usas en ApplicationDbContext
            // services.AddScoped<AuditService>();

            if (!string.IsNullOrWhiteSpace(pg))
            {
                services.AddDbContext<PostgresDbContext>(opt =>
                    opt.UseNpgsql(pg, n =>
                    {
                        n.MigrationsAssembly(typeof(PostgresDbContext).Assembly.FullName);
                        n.EnableRetryOnFailure();
                        n.CommandTimeout(60);
                    })
                );
            }

            return services;
        }
    }
}
