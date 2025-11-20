using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Tests.Helpers;

namespace TodoApi.Tests.Endpoints;

public class TodoEndpointsTests : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private readonly TodoApiFactory _factory;
    private readonly HttpClient _client;

    public TodoEndpointsTests(TodoApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await TestHelpers.ClearDatabaseAsync(_factory.Services);
    }

    [Fact]
    public async Task GetAllTodos_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllTodos_WithAuth_ShouldReturnUserTodos()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);
        _client.AddAuthToken(token);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.Add(new Todo
        {
            Title = "Test Todo",
            CreatedBy = user.Id,
            IsComplete = false,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        todos.Should().NotBeNull();
        todos.Should().HaveCount(1);
        todos![0].Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetAllTodos_ShouldNotReturnOtherUsersTodos()
    {
        // Arrange
        var user1 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user1@example.com");
        var user2 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user2@example.com");
        var token1 = await TestHelpers.GetAuthTokenAsync(_factory.Services, user1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.Add(new Todo { Title = "User1 Todo", CreatedBy = user1.Id, IsDeleted = false });
        db.Todos.Add(new Todo { Title = "User2 Todo", CreatedBy = user2.Id, IsDeleted = false });
        await db.SaveChangesAsync();

        _client.AddAuthToken(token1);

        // Act
        var response = await _client.GetAsync("/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        todos.Should().NotBeNull();
        todos.Should().HaveCount(1);
        todos![0].Title.Should().Be("User1 Todo");
    }

    [Fact]
    public async Task GetAllTodos_ShouldNotReturnDeletedTodos()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        db.Todos.Add(new Todo { Title = "Active Todo", CreatedBy = user.Id, IsDeleted = false });
        db.Todos.Add(new Todo { Title = "Deleted Todo", CreatedBy = user.Id, IsDeleted = true });
        await db.SaveChangesAsync();

        _client.AddAuthToken(token);

        // Act
        var response = await _client.GetAsync("/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        todos.Should().NotBeNull();
        todos.Should().HaveCount(1);
        todos![0].Title.Should().Be("Active Todo");
    }

    [Fact]
    public async Task GetTodo_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "Test Todo", CreatedBy = user.Id, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token);

        // Act
        var response = await _client.GetAsync($"/todos/{todo.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetTodo_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);
        _client.AddAuthToken(token);

        // Act
        var response = await _client.GetAsync("/todos/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTodo_FromAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        var user1 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user1@example.com");
        var user2 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user2@example.com");
        var token1 = await TestHelpers.GetAuthTokenAsync(_factory.Services, user1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "User2 Todo", CreatedBy = user2.Id, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token1);

        // Act
        var response = await _client.GetAsync($"/todos/{todo.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTodo_WithValidRequest_ShouldCreateTodo()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);
        _client.AddAuthToken(token);

        var request = new CreateTodoRequest("New Todo");

        // Act
        var response = await _client.PostAsJsonAsync("/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Todo");
        result.IsComplete.Should().BeFalse();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/todos/{result.Id}");
    }

    [Fact]
    public async Task UpdateTodo_WithValidRequest_ShouldUpdateTodo()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "Original Title", CreatedBy = user.Id, IsComplete = false, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token);

        var request = new UpdateTodoRequest("Updated Title", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/todos/{todo.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTodo_PartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "Original Title", CreatedBy = user.Id, IsComplete = false, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token);

        var request = new UpdateTodoRequest(null, true);

        // Act
        var response = await _client.PutAsJsonAsync($"/todos/{todo.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Original Title");
        result.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTodo_NotOwned_ShouldReturnNotFound()
    {
        // Arrange
        var user1 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user1@example.com");
        var user2 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user2@example.com");
        var token1 = await TestHelpers.GetAuthTokenAsync(_factory.Services, user1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "User2 Todo", CreatedBy = user2.Id, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token1);

        var request = new UpdateTodoRequest("Hacked Title", null);

        // Act
        var response = await _client.PutAsJsonAsync($"/todos/{todo.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodo_WithValidId_ShouldSoftDeleteTodo()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services);
        var token = await TestHelpers.GetAuthTokenAsync(_factory.Services, user);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "To Delete", CreatedBy = user.Id, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        var todoId = todo.Id;

        _client.AddAuthToken(token);

        // Act
        var response = await _client.DeleteAsync($"/todos/{todoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TodoDb>();
        var deletedTodo = await verifyDb.Todos.FindAsync(todoId);
        deletedTodo.Should().NotBeNull();
        deletedTodo!.IsDeleted.Should().BeTrue();
        deletedTodo.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteTodo_NotOwned_ShouldReturnNotFound()
    {
        // Arrange
        var user1 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user1@example.com");
        var user2 = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user2@example.com");
        var token1 = await TestHelpers.GetAuthTokenAsync(_factory.Services, user1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var todo = new Todo { Title = "User2 Todo", CreatedBy = user2.Id, IsDeleted = false };
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        _client.AddAuthToken(token1);

        // Act
        var response = await _client.DeleteAsync($"/todos/{todo.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
