using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class TodoCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly TodoCacheService _cacheService;

    public TodoCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new TodoCacheService(_memoryCache);
    }

    [Fact]
    public async Task GetUserTodosAsync_WhenCacheIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetUserTodos_ShouldStoreTodosInCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Test Todo 1", IsComplete = false, CreatedBy = userId },
            new() { Id = 2, Title = "Test Todo 2", IsComplete = true, CreatedBy = userId }
        };

        // Act
        _cacheService.SetUserTodos(userId, todos);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Title.Should().Be("Test Todo 1");
        result[1].Title.Should().Be("Test Todo 2");
    }

    [Fact]
    public async Task UpdateTodo_WhenTodoExists_ShouldUpdateInCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Original Title", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        var updatedTodo = new Todo
        {
            Id = 1,
            Title = "Updated Title",
            IsComplete = true,
            CreatedBy = userId
        };

        // Act
        _cacheService.UpdateTodo(userId, updatedTodo);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Title.Should().Be("Updated Title");
        result[0].IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTodo_WhenTodoDoesNotExist_ShouldNotAddToCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Existing Todo", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        var nonExistentTodo = new Todo
        {
            Id = 99,
            Title = "Non-existent Todo",
            IsComplete = false,
            CreatedBy = userId
        };

        // Act
        _cacheService.UpdateTodo(userId, nonExistentTodo);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Id.Should().Be(1);
    }

    [Fact]
    public async Task RemoveTodo_WhenTodoExists_ShouldRemoveFromCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Todo 1", IsComplete = false, CreatedBy = userId },
            new() { Id = 2, Title = "Todo 2", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        // Act
        _cacheService.RemoveTodo(userId, 1);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Id.Should().Be(2);
    }

    [Fact]
    public async Task RemoveTodo_WhenTodoDoesNotExist_ShouldNotAffectCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Todo 1", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        // Act
        _cacheService.RemoveTodo(userId, 99);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddTodo_WhenCacheExists_ShouldAddTodoToCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Existing Todo", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        var newTodo = new Todo
        {
            Id = 2,
            Title = "New Todo",
            IsComplete = false,
            CreatedBy = userId
        };

        // Act
        _cacheService.AddTodo(userId, newTodo);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result!.Should().Contain(t => t.Id == 2 && t.Title == "New Todo");
    }

    [Fact]
    public async Task AddTodo_WhenCacheDoesNotExist_ShouldNotCreateCache()
    {
        // Arrange
        var userId = 1;
        var newTodo = new Todo
        {
            Id = 1,
            Title = "New Todo",
            IsComplete = false,
            CreatedBy = userId
        };

        // Act
        _cacheService.AddTodo(userId, newTodo);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateUserCache_ShouldRemoveUserTodosFromCache()
    {
        // Arrange
        var userId = 1;
        var todos = new List<Todo>
        {
            new() { Id = 1, Title = "Todo 1", IsComplete = false, CreatedBy = userId }
        };
        _cacheService.SetUserTodos(userId, todos);

        // Act
        _cacheService.InvalidateUserCache(userId);
        var result = await _cacheService.GetUserTodosAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Cache_ShouldIsolateBetweenDifferentUsers()
    {
        // Arrange
        var user1Todos = new List<Todo>
        {
            new() { Id = 1, Title = "User 1 Todo", IsComplete = false, CreatedBy = 1 }
        };
        var user2Todos = new List<Todo>
        {
            new() { Id = 2, Title = "User 2 Todo", IsComplete = false, CreatedBy = 2 }
        };

        // Act
        _cacheService.SetUserTodos(1, user1Todos);
        _cacheService.SetUserTodos(2, user2Todos);

        var user1Result = await _cacheService.GetUserTodosAsync(1);
        var user2Result = await _cacheService.GetUserTodosAsync(2);

        // Assert
        user1Result.Should().NotBeNull();
        user1Result.Should().HaveCount(1);
        user1Result![0].Title.Should().Be("User 1 Todo");

        user2Result.Should().NotBeNull();
        user2Result.Should().HaveCount(1);
        user2Result![0].Title.Should().Be("User 2 Todo");
    }
}
