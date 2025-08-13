using Xunit;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain
{
    /// <summary>
    /// Tests for Customer domain entity
    /// Testing business rules and entity behavior
    /// </summary>
    public class CustomerDomainTests
    {
        [Fact]
        public void Customer_CreateWithValidData_ShouldHaveCorrectProperties()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@example.com";
            var phone = "123-456-7890";
            var address = "123 Main St";
            var dateOfBirth = new DateTime(1990, 1, 1);

            // Act
            var customer = new Customer
            {
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                DateOfBirth = dateOfBirth
            };

            // Assert
            Assert.Equal(name, customer.Name);
            Assert.Equal(email, customer.Email);
            Assert.Equal(phone, customer.Phone);
            Assert.Equal(address, customer.Address);
            Assert.Equal(dateOfBirth, customer.DateOfBirth);
            Assert.True(customer.IsActive);
            Assert.True(customer.CreatedAt <= DateTime.UtcNow);
            Assert.True(customer.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Customer_DefaultValues_ShouldBeCorrect()
        {
            // Act
            var customer = new Customer();

            // Assert
            Assert.Equal(string.Empty, customer.Name);
            Assert.Equal(string.Empty, customer.Email);
            Assert.Equal(string.Empty, customer.Phone);
            Assert.Equal(string.Empty, customer.Address);
            Assert.True(customer.IsActive);
            Assert.NotNull(customer.Appointments);
            Assert.Empty(customer.Appointments);
        }

        [Fact]
        public void Customer_SetIsActive_ShouldUpdateCorrectly()
        {
            // Arrange
            var customer = new Customer();

            // Act
            customer.IsActive = false;

            // Assert
            Assert.False(customer.IsActive);
        }
    }
}
