using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JobTrackr.Application.Auth;
using JobTrackr.Application.Comments;
using JobTrackr.Application.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTrackr.Tests.Integration;

public class TaskCommentEndpointTests : IClassFixture<JobTrackrApiFactory>
{
    private readonly JobTrackrApiFactory _factory;

    public TaskCommentEndpointTests(JobTrackrApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CommentWorkflow_WithOwner_ReturnsExpectedResponses()
    {
        using var client = CreateClient();
        await AuthenticateAsync(client);
        var taskId = await CreateTaskAsync(client);
        var path = $"/api/tasks/{taskId}/comments";

        var empty = await client.GetFromJsonAsync<List<TaskCommentResponse>>(path);
        Assert.NotNull(empty);
        Assert.Empty(empty);

        using var invalid = await client.PostAsJsonAsync(path,
            new CreateTaskCommentRequest { Content = " " });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        using var created = await client.PostAsJsonAsync(path,
            new CreateTaskCommentRequest { Content = "  HTTP comment  " });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        Assert.Equal(path, created.Headers.Location.OriginalString);
        var comment = await created.Content.ReadFromJsonAsync<TaskCommentResponse>();
        Assert.NotNull(comment);
        Assert.True(comment.Id > 0);
        Assert.Equal(taskId, comment.TaskId);
        Assert.Equal("HTTP comment", comment.Content);

        var listed = await client.GetFromJsonAsync<List<TaskCommentResponse>>(path);
        Assert.NotNull(listed);
        Assert.Equal(comment.Id, Assert.Single(listed).Id);

        using var deleted = await client.DeleteAsync($"{path}/{comment.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty(await deleted.Content.ReadAsStringAsync());
        var after = await client.GetFromJsonAsync<List<TaskCommentResponse>>(path);
        Assert.NotNull(after);
        Assert.Empty(after);

        using var missing = await client.DeleteAsync($"{path}/{comment.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        var problem = await missing.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Comment not found.", problem.Detail);

        using var taskResponse = await client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.OK, taskResponse.StatusCode);
    }

    [Fact]
    public async Task CommentEndpoints_WithOtherUserOrAnonymous_DenyAccess()
    {
        using var owner = CreateClient();
        await AuthenticateAsync(owner);
        var taskId = await CreateTaskAsync(owner);
        var path = $"/api/tasks/{taskId}/comments";
        using var created = await owner.PostAsJsonAsync(path,
            new CreateTaskCommentRequest { Content = "Private note" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var comment = await created.Content.ReadFromJsonAsync<TaskCommentResponse>();
        Assert.NotNull(comment);

        using var other = CreateClient();
        await AuthenticateAsync(other);
        using var deniedGet = await other.GetAsync(path);
        using var deniedCreate = await other.PostAsJsonAsync(path,
            new CreateTaskCommentRequest { Content = "Not permitted" });
        using var deniedDelete = await other.DeleteAsync($"{path}/{comment.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deniedGet.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deniedCreate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deniedDelete.StatusCode);

        using var anonymous = CreateClient();
        using var anonymousGet = await anonymous.GetAsync(path);
        using var anonymousCreate = await anonymous.PostAsJsonAsync(path,
            new CreateTaskCommentRequest { Content = "Not permitted" });
        using var anonymousDelete = await anonymous.DeleteAsync($"{path}/{comment.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousCreate.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousDelete.StatusCode);

        var retained = await owner.GetFromJsonAsync<List<TaskCommentResponse>>(path);
        Assert.NotNull(retained);
        Assert.Equal(comment.Id, Assert.Single(retained).Id);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task AuthenticateAsync(HttpClient client)
    {
        var email = $"comment-http-{Guid.NewGuid():N}@example.com";
        const string password = "IntegrationTest123!";
        using var registered = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest
            {
                FullName = "Comment HTTP User",
                Email = email,
                Password = password
            });
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);

        using var loggedIn = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.OK, loggedIn.StatusCode);
        var auth = await loggedIn.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);
    }

    private static async Task<int> CreateTaskAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest { Title = "HTTP comment task", Priority = "Medium" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        return task.Id;
    }
}