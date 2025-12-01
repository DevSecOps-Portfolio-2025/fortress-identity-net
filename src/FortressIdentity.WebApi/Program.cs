using System.Text;
using FortressIdentity.Application;
using FortressIdentity.Infrastructure;
using FortressIdentity.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.

// Register Application services (MediatR, FluentValidation, etc.)
builder.Services.AddApplication();

// Register Infrastructure services (Database Context, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found in configuration.");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

builder.Services.AddAuthorization();

// Register Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT aquí. Ejemplo: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Use Serilog request logging (early in the pipeline)
// 1. Inicia el log de Serilog, pero configurado para extraer datos extra
app.UseSerilogRequestLogging(options =>
{
    // Esta función se ejecuta JUSTO antes de escribir el log final
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        // Si el usuario está autenticado...
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            // Busca el ID (puede estar como NameIdentifier o como "sub")
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                         ?? httpContext.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // ¡Inyectalo en el log final!
                diagnosticContext.Set("UserId", userId);
            }
            
            // Opcional: También el Email si quieres verlo fácil
            var email = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                diagnosticContext.Set("UserEmail", email);
            }
        }
    };
});

// Use global exception handler
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Enrich logs with user context (after authentication, before controllers)
app.UseMiddleware<UserContextLoggingMiddleware>();

app.MapControllers();

app.Run();
