using System.Net.Http.Json;
using AmsApi.DTOs;
using AmsApi.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace AmsApi.Tests;

public class SubjectIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SubjectIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSubject_Should_Return_201_And_Valid_Subject()
    {
        // Arrange
        var dto = new CreateSubjectDto
        {
            Name = "Math",
            Description = "Algebra & Geometry"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/subjects", dto);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<SubjectDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Math");
    }
}
