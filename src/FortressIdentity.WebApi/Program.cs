using FortressIdentity.Application;
using FortressIdentity.Infrastructure;
using FortressIdentity.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Application services (MediatR, FluentValidation, etc.)
builder.Services.AddApplication();

// Register Infrastructure services (Database Context, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Register Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Use global exception handler
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
