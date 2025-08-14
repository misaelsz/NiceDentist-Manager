using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Infrastructure.Repositories;
using NiceDentist.Manager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add OpenAPI/Swagger services
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "NiceDentist Manager API", 
        Version = "v1",
        Description = "API for managing customers, dentists, and appointments in the NiceDentist system"
    });
});

// Register application services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Register repositories based on environment
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider");

if (databaseProvider == "InMemory" || builder.Environment.IsDevelopment())
{
    // Development: Use in-memory implementations
    builder.Services.AddScoped<ICustomerRepository, InMemoryCustomerRepository>();
    builder.Services.AddScoped<IAppointmentRepository, InMemoryAppointmentRepository>();
}
else
{
    // Production: Use real database implementations
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>(); // Real SQL implementation
    builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>(); // Real SQL implementation
}

// Register other services (temporary mock implementations)
builder.Services.AddScoped<IAuthApiService, MockAuthApiService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiceDentist Manager API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}
else
{
    // Enable Swagger in production for testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiceDentist Manager API v1");
        c.RoutePrefix = "swagger"; // Available at /swagger
    });
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
