using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackr.Tests.Integration;

public class HealthEndpointTests : IClassFixture<JobTrackrApiFactory>
{
    private readonly JobTrackrApiFactory _factory;

    public HealthEndpointTests(JobTrackrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyBody()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }
}