using Xunit;

namespace NiceDentist.Manager.Tests
{
    /// <summary>
    /// Simple test to verify the test framework is working correctly
    /// This should always pass and confirms our test setup is functional
    /// </summary>
    public class TestFrameworkValidation
    {
        [Fact]
        public void TestFramework_ShouldWork()
        {
            // Arrange
            var expected = 42;
            var actual = 40 + 2;

            // Act & Assert
            Assert.Equal(expected, actual);
            Assert.True(true);
            Assert.False(false);
            Assert.NotNull("test");
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 3, 5)]
        [InlineData(-1, 1, 0)]
        public void TestFramework_Theory_ShouldWork(int a, int b, int expected)
        {
            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
