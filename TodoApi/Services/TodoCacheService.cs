using Microsoft.Extensions.Caching.Memory;
using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoCacheService
{
    Task<List<Todo>?> GetUserTodosAsync(int userId);
    void SetUserTodos(int userId, List<Todo> todos);
    void UpdateTodo(int userId, Todo todo);
    void RemoveTodo(int userId, int todoId);
    void AddTodo(int userId, Todo todo);
    void InvalidateUserCache(int userId);
}

public class TodoCacheService : ITodoCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(2);

    public TodoCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private string GetCacheKey(int userId) => $"user_todos_{userId}";

    public Task<List<Todo>?> GetUserTodosAsync(int userId)
    {
        _cache.TryGetValue(GetCacheKey(userId), out List<Todo>? todos);
        return Task.FromResult(todos);
    }

    public void SetUserTodos(int userId, List<Todo> todos)
    {
        var cacheKey = GetCacheKey(userId);
        _cache.Set(cacheKey, todos, new MemoryCacheEntryOptions
        {
            SlidingExpiration = _cacheExpiration
        });
    }

    public void UpdateTodo(int userId, Todo todo)
    {
        var cacheKey = GetCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<Todo>? todos) && todos != null)
        {
            var existingTodo = todos.FirstOrDefault(t => t.Id == todo.Id);
            if (existingTodo != null)
            {
                todos.Remove(existingTodo);
                todos.Add(todo);
                SetUserTodos(userId, todos);
            }
        }
    }

    public void RemoveTodo(int userId, int todoId)
    {
        var cacheKey = GetCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<Todo>? todos) && todos != null)
        {
            var todoToRemove = todos.FirstOrDefault(t => t.Id == todoId);
            if (todoToRemove != null)
            {
                todos.Remove(todoToRemove);
                SetUserTodos(userId, todos);
            }
        }
    }

    public void AddTodo(int userId, Todo todo)
    {
        var cacheKey = GetCacheKey(userId);
        if (_cache.TryGetValue(cacheKey, out List<Todo>? todos) && todos != null)
        {
            todos.Add(todo);
            SetUserTodos(userId, todos);
        }
    }

    public void InvalidateUserCache(int userId)
    {
        _cache.Remove(GetCacheKey(userId));
    }
}
