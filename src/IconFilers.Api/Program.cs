using IconFilers.Api.IServices;
using IconFilers.Api.Services;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure EPPlus license context for reading Excel files (NonCommercial for development)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Read connection string from appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Jwt settings must come from configuration (appsettings or env vars)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? throw new ArgumentNullException("Jwt:Secret not configured");
var jwtIssuer = jwtSection["Issuer"] ?? "IconFilers";
var jwtAudience = jwtSection["Audience"] ?? "IconFilers";
var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var m) ? m : 60;

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

// Configure authentication. In Development use a simple auto-authentication handler
// so local development does not require a real JWT token. In Production use JWT.
if (builder.Environment.IsDevelopment())
{
    // A lightweight development-only authentication scheme that auto-authenticates
    // requests as a default development user with Admin role.
    builder.Services.AddAuthentication("Development")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, IconFilers.Api.Services.DevelopmentAuthenticationHandler>(
            "Development", options => { });
}
else
{
    // Configure JWT authentication for non-development environments
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
    });
}

// Add authorization services (policies are defined via attributes on controllers)
builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IWorkflow, WorkFlowService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IManageTeamsService, ManageTeamsService>();
builder.Services.AddScoped<IClientAssignmentService, ClientAssignmentService>();
builder.Services.AddScoped<IClientDocumentService, ClientDocumentService>();
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));

builder.Services.AddHttpClient<PayPalPaymentService>();
builder.Services.AddScoped<IPaymentService, PayPalPaymentService>();

// Add JwtService that reads configuration
builder.Services.AddSingleton<IJwtService, JwtService>();

// Register password hasher for User entity
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<IconFilers.Infrastructure.Persistence.Entities.User>, Microsoft.AspNetCore.Identity.PasswordHasher<IconFilers.Infrastructure.Persistence.Entities.User>>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "IconFilers API",
        Version = "v1"
    });

    // Add JWT Bearer support to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\nExample: \"Bearer eyJhb...\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    };

    options.AddSecurityRequirement(securityRequirement);
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

app.UseSwaggerUI();

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Serve wwwroot/static files
app.UseStaticFiles();

// Register controllers
app.MapControllers();

// Run app
app.Run();
