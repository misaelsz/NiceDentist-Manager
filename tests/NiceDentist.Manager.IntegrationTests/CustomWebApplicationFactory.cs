using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Infrastructure.Repositories;
using System.Linq;

namespace NiceDentist.Manager.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// </summary>
/// <typeparam name="TProgram">The program type</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Configure in-memory configuration for tests
            var inMemoryConfig = new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Data Source=InMemory;Mode=Memory;Cache=Shared"},
                {"RabbitMQ:ConnectionString", "amqp://guest:guest@localhost:5672/"},
                {"Logging:LogLevel:Default", "Warning"}
            };
            config.AddInMemoryCollection(inMemoryConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Remove RabbitMQ related services and hosted services
            var servicesToRemove = services.Where(d =>
                d.ServiceType.FullName?.Contains("RabbitMq") == true ||
                d.ServiceType == typeof(IHostedService) ||
                d.ImplementationType?.FullName?.Contains("RabbitMq") == true).ToList();

            foreach (var service in servicesToRemove)
            {
                services.Remove(service);
            }

            // Replace SQL repositories with in-memory ones for testing
            var customerRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICustomerRepository));
            var dentistRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDentistRepository));
            var appointmentRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAppointmentRepository));

            if (customerRepoDescriptor != null)
                services.Remove(customerRepoDescriptor);
            if (dentistRepoDescriptor != null)
                services.Remove(dentistRepoDescriptor);
            if (appointmentRepoDescriptor != null)
                services.Remove(appointmentRepoDescriptor);

            services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
            services.AddSingleton<IDentistRepository, InMemoryDentistRepository>();
            services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the repositories
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

            try
            {
                // Seed the repositories with test data
                SeedTestData(scopedServices, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test data");
            }
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Seeds the test repositories with initial data
    /// </summary>
    /// <param name="services">The service provider</param>
    /// <param name="logger">The logger</param>
    private static void SeedTestData(IServiceProvider services, ILogger logger)
    {
        try
        {
            var customerRepo = services.GetRequiredService<ICustomerRepository>();
            var dentistRepo = services.GetRequiredService<IDentistRepository>();
            var appointmentRepo = services.GetRequiredService<IAppointmentRepository>();

            // Seed customers
            var customer1 = new Domain.Customer
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "123-456-7890",
                DateOfBirth = new DateTime(1990, 1, 15),
                Address = "123 Main St",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var customer2 = new Domain.Customer
            {
                Id = 2,
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                Phone = "987-654-3210",
                DateOfBirth = new DateTime(1985, 6, 20),
                Address = "456 Oak Ave",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            customerRepo.CreateAsync(customer1).Wait();
            customerRepo.CreateAsync(customer2).Wait();

            // Seed dentists
            var dentist1 = new Domain.Dentist
            {
                Id = 1,
                Name = "Dr. Michael Johnson",
                Email = "dr.johnson@nicedentist.com",
                Phone = "555-123-4567",
                LicenseNumber = "DDS123456",
                Specialization = "General Dentistry",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var dentist2 = new Domain.Dentist
            {
                Id = 2,
                Name = "Dr. Sarah Wilson",
                Email = "dr.wilson@nicedentist.com",
                Phone = "555-987-6543",
                LicenseNumber = "DDS789012",
                Specialization = "Orthodontics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dentistRepo.CreateAsync(dentist1).Wait();
            dentistRepo.CreateAsync(dentist2).Wait();

            // Seed appointments
            var appointment1 = new Domain.Appointment
            {
                Id = 1,
                CustomerId = 1,
                DentistId = 1,
                AppointmentDateTime = DateTime.UtcNow.AddDays(7),
                ProcedureType = "Regular cleaning and checkup",
                Notes = "Routine visit",
                Status = Domain.AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var appointment2 = new Domain.Appointment
            {
                Id = 2,
                CustomerId = 2,
                DentistId = 2,
                AppointmentDateTime = DateTime.UtcNow.AddDays(14),
                ProcedureType = "Orthodontic consultation",
                Notes = "Initial consultation",
                Status = Domain.AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            appointmentRepo.CreateAsync(appointment1).Wait();
            appointmentRepo.CreateAsync(appointment2).Wait();

            logger.LogInformation("Test data seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding test data");
        }
    }
}
