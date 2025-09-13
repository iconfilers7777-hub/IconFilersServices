using Microsoft.Extensions.DependencyInjection;

namespace IconFilers.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Example: Repository bindings
            //services.AddScoped<IExampleRepository, ExampleRepository>();

            // Example: DbContext (if using EF Core)
            // services.AddDbContext<AppDbContext>(options =>
            //     options.UseSqlServer("YourConnectionString"));

            return services;
        }
    }
}
