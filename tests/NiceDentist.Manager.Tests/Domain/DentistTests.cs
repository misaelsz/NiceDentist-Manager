using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Dentist domain entity
/// </summary>
public class DentistTests
{
    [Fact]
    public void Dentist_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var dentist = new Dentist();

        // Assert
        dentist.Id.Should().Be(0);
        dentist.Name.Should().BeEmpty();
        dentist.Email.Should().BeEmpty();
        dentist.Phone.Should().BeEmpty();
        dentist.LicenseNumber.Should().BeEmpty();
        dentist.Specialization.Should().BeEmpty();
        dentist.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dentist.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dentist.IsActive.Should().BeTrue();
        dentist.Appointments.Should().NotBeNull();
        dentist.Appointments.Should().BeEmpty();
    }

    [Fact]
    public void Dentist_SetProperties_ShouldSetCorrectly()
    {
        // Arrange
        var dentist = new Dentist();
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        dentist.Id = 1;
        dentist.Name = "Dr. Jane Smith";
        dentist.Email = "dr.jane@dentalclinic.com";
        dentist.Phone = "+1987654321";
        dentist.LicenseNumber = "DDS-12345";
        dentist.Specialization = "Orthodontics";
        dentist.CreatedAt = createdAt;
        dentist.UpdatedAt = updatedAt;
        dentist.IsActive = false;

        // Assert
        dentist.Id.Should().Be(1);
        dentist.Name.Should().Be("Dr. Jane Smith");
        dentist.Email.Should().Be("dr.jane@dentalclinic.com");
        dentist.Phone.Should().Be("+1987654321");
        dentist.LicenseNumber.Should().Be("DDS-12345");
        dentist.Specialization.Should().Be("Orthodontics");
        dentist.CreatedAt.Should().Be(createdAt);
        dentist.UpdatedAt.Should().Be(updatedAt);
        dentist.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("DDS-12345")]
    [InlineData("CRO-SP-67890")]
    [InlineData("LIC123456")]
    [InlineData("12345")]
    public void Dentist_LicenseNumberFormats_ShouldBeAccepted(string licenseNumber)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.LicenseNumber = licenseNumber;

        // Assert
        dentist.LicenseNumber.Should().Be(licenseNumber);
    }

    [Theory]
    [InlineData("Orthodontics")]
    [InlineData("Endodontics")]
    [InlineData("Periodontics")]
    [InlineData("Oral Surgery")]
    [InlineData("Pediatric Dentistry")]
    [InlineData("General Dentistry")]
    [InlineData("Cosmetic Dentistry")]
    public void Dentist_Specializations_ShouldBeAccepted(string specialization)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Specialization = specialization;

        // Assert
        dentist.Specialization.Should().Be(specialization);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Dentist_EmptySpecialization_ShouldBeHandled(string? specialization)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Specialization = specialization ?? string.Empty;

        // Assert
        dentist.Specialization.Should().Be(specialization ?? string.Empty);
    }

    [Fact]
    public void Dentist_AppointmentsCollection_ShouldBeModifiable()
    {
        // Arrange
        var dentist = new Dentist();
        var appointment1 = new Appointment();
        var appointment2 = new Appointment();

        // Act
        dentist.Appointments.Add(appointment1);
        dentist.Appointments.Add(appointment2);

        // Assert
        dentist.Appointments.Should().HaveCount(2);
        dentist.Appointments.Should().Contain(appointment1);
        dentist.Appointments.Should().Contain(appointment2);
    }

    [Theory]
    [InlineData("Dr. John Smith")]
    [InlineData("Jane Doe, DDS")]
    [InlineData("Dr. Maria Garcia-Lopez")]
    [InlineData("Robert Johnson III")]
    public void Dentist_NameFormats_ShouldBeAccepted(string name)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Name = name;

        // Assert
        dentist.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("doctor@clinic.com")]
    [InlineData("dr.smith@dentalgroup.org")]
    [InlineData("dentist123@hospital.co.uk")]
    public void Dentist_ProfessionalEmails_ShouldBeAccepted(string email)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Email = email;

        // Assert
        dentist.Email.Should().Be(email);
    }

    [Fact]
    public void Dentist_TimestampProperties_ShouldBeIndependent()
    {
        // Arrange
        var dentist = new Dentist();
        var createdTime = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        dentist.CreatedAt = createdTime;
        dentist.UpdatedAt = updatedTime;

        // Assert
        dentist.CreatedAt.Should().Be(createdTime);
        dentist.UpdatedAt.Should().Be(updatedTime);
        dentist.UpdatedAt.Should().BeAfter(dentist.CreatedAt);
    }

    [Fact]
    public void Dentist_IsActive_ShouldControlVisibility()
    {
        // Arrange
        var dentist = new Dentist();

        // Act & Assert - Initially active
        dentist.IsActive.Should().BeTrue();

        // Act - Suspend dentist
        dentist.IsActive = false;

        // Assert
        dentist.IsActive.Should().BeFalse();

        // Act - Reactivate dentist
        dentist.IsActive = true;

        // Assert
        dentist.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("(555) 987-6543")]
    [InlineData("555.987.6543")]
    [InlineData("55598766543")]
    [InlineData("+44 20 7946 0958")]
    public void Dentist_InternationalPhoneFormats_ShouldBeAccepted(string phone)
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Phone = phone;

        // Assert
        dentist.Phone.Should().Be(phone);
    }
}
