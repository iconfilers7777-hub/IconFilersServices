using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using IconFilers.Application.DTOs;
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
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));

builder.Services.AddHttpClient<PayPalPaymentService>();
builder.Services.AddScoped<IPaymentService, PayPalPaymentService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "IconFilers API",
        Version = "v1"
    });
});

// Add Infrastructure
builder.Services.AddInfrastructureServices(connectionString);

// Enable CORS for all
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

// Swagger must work for both Development & Production (IIS)
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    string basePath = "/IconFilers"; // <-- your virtual directory name

    c.SwaggerEndpoint($"{basePath}/swagger/v1/swagger.json", "IconFilers API v1");

    c.RoutePrefix = "swagger";
});

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthorization();

// Serve wwwroot/static files
app.UseStaticFiles();

// Register controllers
app.MapControllers();

// Run app
app.Run();
