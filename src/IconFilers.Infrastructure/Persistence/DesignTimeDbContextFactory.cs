using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IconFilers.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private const string DefaultConnectionName = "DefaultConnection";

        public AppDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Candidate directories to search for appsettings.json
            var candidates = new[]
            {
                // Infrastructure project directory (when running from infra)
                Directory.GetCurrentDirectory(),

                // typical solution layout: Infrastructure next to Api
                Path.Combine(Directory.GetCurrentDirectory(), "..", "IconFilers.Api"),

                // when running from solution root
                Path.Combine(Directory.GetCurrentDirectory(), "src", "IconFilers.Api"),

                // relative from bin folders (two levels up then src)
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "IconFilers.Api")
            };

            IConfigurationRoot? configuration = null;
            string? usedBasePath = null;

            foreach (var candidate in candidates)
            {
                try
                {
                    var full = Path.GetFullPath(candidate);
                    var json = Path.Combine(full, "appsettings.json");
                    var jsonEnv = Path.Combine(full, $"appsettings.{env}.json");

                    if (File.Exists(json) || File.Exists(jsonEnv))
                    {
                        var builder = new ConfigurationBuilder()
                            .SetBasePath(full)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                            .AddEnvironmentVariables();

                        configuration = builder.Build();
                        usedBasePath = full;
                        Console.WriteLine($"DesignTimeDbContextFactory: found config at: {full}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // don't fail here - continue to next candidate
                    Console.WriteLine($"DesignTimeDbContextFactory: candidate '{candidate}' threw: {ex.Message}");
                }
            }

            string? connectionString = configuration?.GetConnectionString(DefaultConnectionName);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
