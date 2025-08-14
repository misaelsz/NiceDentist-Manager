using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Infrastructure.Repositories;
using NiceDentist.Manager.Infrastructure.Services;
using NiceDentist.Manager.Infrastructure.Messaging;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

if (databaseProvider == "InMemory")
{
    // Development: Use in-memory implementations (Singleton to persist data between requests)
    builder.Services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
    builder.Services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();
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

// Register RabbitMQ services
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = config.GetValue<string>("RabbitMQ:HostName") ?? "localhost",
        Port = config.GetValue<int>("RabbitMQ:Port", 5672),
        UserName = config.GetValue<string>("RabbitMQ:UserName") ?? "guest",
        Password = config.GetValue<string>("RabbitMQ:Password") ?? "guest",
        VirtualHost = config.GetValue<string>("RabbitMQ:VirtualHost") ?? "/"
    };
});

builder.Services.AddScoped<IEventPublisher>(sp =>
{
    var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
    var config = sp.GetRequiredService<IConfiguration>();
    var exchangeName = config.GetValue<string>("RabbitMQ:ExchangeName") ?? "nicedentist.events";
    return new RabbitMqEventPublisher(connectionFactory, exchangeName);
});

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

// Enable CORS
app.UseCors("AllowFrontend");

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
