using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using IconFilers.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// EPPlus license
ExcelPackage.License.SetNonCommercialPersonal("Venkatesh");
ExcelPackage.License.SetNonCommercialOrganization("IconFilers");

// Read connection string from appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Generate secret key for JWT
var keyBytes = RandomNumberGenerator.GetBytes(32); // 256 bits
var secretKey = Convert.ToBase64String(keyBytes);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Dependency Injection
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IWorkflow, WorkFlowService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfRepository<>));
builder.Services.AddSingleton<IJwtService>(new JwtService(secretKey));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IManageTeamsService, ManageTeamsService>();
builder.Services.AddScoped<IClientAssignmentService, ClientAssignmentService>();
builder.Services.AddScoped<IClientDocumentService, ClientDocumentService>();


// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IconFilers API",
        Version = "v1"
    });
});

// Add Infrastructure
builder.Services.AddInfrastructureServices(connectionString);

// ✅ Add CORS policy to allow all URLs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IconFilers API v1"));
}

// Enable HTTPS, CORS, and routing
app.UseHttpsRedirection();

// ✅ Use CORS policy
app.UseCors("AllowAll");

app.UseAuthorization();
app.UseStaticFiles(); // serve files from wwwroot
app.MapControllers();
app.Run();
