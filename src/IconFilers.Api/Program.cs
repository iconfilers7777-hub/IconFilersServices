using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using IconFilers.Infrastructure.DependencyInjection;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);


ExcelPackage.License.SetNonCommercialPersonal("Venkatesh");


ExcelPackage.License.SetNonCommercialOrganization("IconFilers");

// read connection string from appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var keyBytes = RandomNumberGenerator.GetBytes(32); // 256 bits
var secretKey = Convert.ToBase64String(keyBytes);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClientService,ClientService>();
builder.Services.AddScoped<IWorkflow,WorkFlowService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfRepository<>));
builder.Services.AddSingleton<IJwtService>(new JwtService(secretKey));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IconFilers API",
        Version = "v1"
    });

    // Optional: include XML comments (if you generate them)
    // var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});

builder.Services.AddInfrastructureServices(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IconFilers API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
