using Xunit;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain
{
    /// <summary>
    /// Tests for Dentist domain entity
    /// Testing business rules and entity behavior
    /// </summary>
    public class DentistDomainTests
    {
        [Fact]
        public void Dentist_CreateWithValidData_ShouldHaveCorrectProperties()
        {
            // Arrange
            var name = "Dr. Smith";
            var email = "dr.smith@clinic.com";
            var phone = "987-654-3210";
            var licenseNumber = "LIC123456";
            var specialization = "General Dentistry";

            // Act
            var dentist = new Dentist
            {
                Name = name,
                Email = email,
                Phone = phone,
                LicenseNumber = licenseNumber,
                Specialization = specialization
            };

            // Assert
            Assert.Equal(name, dentist.Name);
            Assert.Equal(email, dentist.Email);
            Assert.Equal(phone, dentist.Phone);
            Assert.Equal(licenseNumber, dentist.LicenseNumber);
            Assert.Equal(specialization, dentist.Specialization);
            Assert.True(dentist.IsActive);
            Assert.True(dentist.CreatedAt <= DateTime.UtcNow);
            Assert.True(dentist.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Dentist_DefaultValues_ShouldBeCorrect()
        {
            // Act
            var dentist = new Dentist();

            // Assert
            Assert.Equal(string.Empty, dentist.Name);
            Assert.Equal(string.Empty, dentist.Email);
            Assert.Equal(string.Empty, dentist.Phone);
            Assert.Equal(string.Empty, dentist.LicenseNumber);
            Assert.Equal(string.Empty, dentist.Specialization);
            Assert.True(dentist.IsActive);
            Assert.NotNull(dentist.Appointments);
            Assert.Empty(dentist.Appointments);
        }

        [Fact]
        public void Dentist_SetSpecialization_ShouldUpdateCorrectly()
        {
            // Arrange
            var dentist = new Dentist();
            var newSpecialization = "Orthodontics";

            // Act
            dentist.Specialization = newSpecialization;

            // Assert
            Assert.Equal(newSpecialization, dentist.Specialization);
        }

        [Fact]
        public void Dentist_SetLicenseNumber_ShouldUpdateCorrectly()
        {
            // Arrange
            var dentist = new Dentist();
            var licenseNumber = "LIC789012";

            // Act
            dentist.LicenseNumber = licenseNumber;

            // Assert
            Assert.Equal(licenseNumber, dentist.LicenseNumber);
        }
    }
}
