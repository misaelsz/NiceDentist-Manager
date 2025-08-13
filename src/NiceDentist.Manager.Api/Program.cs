using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Infrastructure.Repositories;
using NiceDentist.Manager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register application services
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Register repositories (temporary in-memory implementations)
builder.Services.AddScoped<ICustomerRepository, InMemoryCustomerRepository>();

// Register other services (temporary mock implementations)
builder.Services.AddScoped<IAuthApiService, MockAuthApiService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

await app.RunAsync();

/// <summary>
/// Program class for integration tests
/// </summary>
public partial class Program 
{ 
    /// <summary>
    /// Protected constructor for testing
    /// </summary>
    protected Program() { }
}
