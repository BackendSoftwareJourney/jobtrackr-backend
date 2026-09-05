using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JobTrackr.Application.Auth;
using JobTrackr.Application.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackr.Tests.Integration;

public class TaskEndpointTests : IClassFixture<JobTrackrApiFactory>
{
    private readonly JobTrackrApiFactory _factory;

    public TaskEndpointTests(JobTrackrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateGetAndUpdateTask_WithAuthenticatedUser_ReturnsExpectedResponses()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var authenticatedUser = await RegisterAndLoginAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authenticatedUser.Token);

        var dueDateUtc = DateTime.UtcNow.AddDays(5);

        var createRequest = new CreateTaskRequest
        {
            Title = "Prepare integration test",
            Description = "Create and retrieve a task through HTTP.",
            DueDateUtc = dueDateUtc,
            Priority = "High"
        };

        using var createResponse = await client.PostAsJsonAsync(
            "/api/tasks",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdTask = await createResponse.Content
            .ReadFromJsonAsync<TaskResponse>();

        Assert.NotNull(createdTask);
        Assert.True(createdTask.Id > 0);
        Assert.Equal(authenticatedUser.UserId, createdTask.UserId);
        Assert.Equal(createRequest.Title, createdTask.Title);
        Assert.Equal(createRequest.Description, createdTask.Description);
        Assert.Equal(createRequest.DueDateUtc, createdTask.DueDateUtc);
        Assert.Equal(createRequest.Priority, createdTask.Priority);
        Assert.False(createdTask.IsCompleted);

        using var getResponse = await client.GetAsync(
            $"/api/tasks/{createdTask.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedTask = await getResponse.Content
            .ReadFromJsonAsync<TaskResponse>();

        Assert.NotNull(fetchedTask);
        Assert.Equal(createdTask.Id, fetchedTask.Id);
        Assert.Equal(createdTask.UserId, fetchedTask.UserId);
        Assert.Equal(createRequest.Title, fetchedTask.Title);

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated integration task",
            Description = "The task was updated through HTTP.",
            DueDateUtc = dueDateUtc.AddDays(2),
            Priority = "Medium"
        };

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/tasks/{createdTask.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedTask = await updateResponse.Content
            .ReadFromJsonAsync<TaskResponse>();

        Assert.NotNull(updatedTask);
        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal(authenticatedUser.UserId, updatedTask.UserId);
        Assert.Equal(updateRequest.Title, updatedTask.Title);
        Assert.Equal(updateRequest.Description, updatedTask.Description);
        Assert.Equal(updateRequest.DueDateUtc, updatedTask.DueDateUtc);
        Assert.Equal(updateRequest.Priority, updatedTask.Priority);
        Assert.False(updatedTask.IsCompleted);
    }

    private static async Task<AuthResponse> RegisterAndLoginAsync(
        HttpClient client)
    {
        var email = $"task-{Guid.NewGuid():N}@example.com";
        const string password = "IntegrationTest123!";

        var registerRequest = new RegisterRequest
        {
            FullName = "Task Integration User",
            Email = email,
            Password = password
        };

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registerRequest);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authenticatedUser = await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authenticatedUser);
        Assert.False(string.IsNullOrWhiteSpace(authenticatedUser.Token));

        return authenticatedUser;
    }
}