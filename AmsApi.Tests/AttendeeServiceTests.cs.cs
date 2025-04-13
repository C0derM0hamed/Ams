using AmsApi.Services;
using AmsApi.DTOs;
using AmsApi.Config;
using Microsoft.Extensions.Options;
using Xunit;

namespace AmsApi.Tests;

public class AttendeeServiceTests
{
    private AttendeeService GetService()
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "testkeytestkeytestkeytestkey",
            Issuer = "test",
            Audience = "test",
            ExpiryInMinutes = 30
        });

        return new AttendeeService(jwtSettings);
    }

    [Fact]
    public async Task RegisterAsync_Should_Add_New_Attendee()
    {
        // Arrange
        var service = GetService();
        var dto = new RegisterAttendeeDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "123456"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }
}
