using System.Net;
using System.Net.Http.Json;
using JobTrackr.Application.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackr.Tests.Integration;

public class AuthEndpointTests : IClassFixture<JobTrackrApiFactory>
{
    private readonly JobTrackrApiFactory _factory;

    public AuthEndpointTests(JobTrackrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAndLogin_WithValidCredentials_ReturnsUserAndToken()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var registerRequest = new RegisterRequest
        {
            FullName = "Authentication Integration User",
            Email = $"auth-{Guid.NewGuid():N}@example.com",
            Password = "IntegrationTest123!"
        };

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registerRequest);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registeredUser = await registerResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(registeredUser);
        Assert.True(registeredUser.UserId > 0);
        Assert.Equal(registerRequest.FullName, registeredUser.FullName);
        Assert.Equal(registerRequest.Email, registeredUser.Email);
        Assert.Equal(string.Empty, registeredUser.Token);

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loggedInUser = await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(loggedInUser);
        Assert.Equal(registeredUser.UserId, loggedInUser.UserId);
        Assert.Equal(registerRequest.FullName, loggedInUser.FullName);
        Assert.Equal(registerRequest.Email, loggedInUser.Email);
        Assert.False(string.IsNullOrWhiteSpace(loggedInUser.Token));
    }
}