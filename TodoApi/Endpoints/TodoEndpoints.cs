using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var todos = app.MapGroup("/todos")
            .WithTags("Todos")
            .RequireAuthorization();

        todos.MapGet("/", GetAllTodos)
            .WithName("GetAllTodos");

        todos.MapGet("/{id}", GetTodo)
            .WithName("GetTodo");

        todos.MapPost("/", CreateTodo)
            .WithName("CreateTodo");

        todos.MapPut("/{id}", UpdateTodo)
            .WithName("UpdateTodo");

        todos.MapDelete("/{id}", DeleteTodo)
            .WithName("DeleteTodo");
    }

    private static async Task<IResult> GetAllTodos(ClaimsPrincipal user, TodoDb db, ITodoCacheService cacheService)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Try to get from cache first
        var cachedTodos = await cacheService.GetUserTodosAsync(userId);
        if (cachedTodos != null)
        {
            return TypedResults.Ok(cachedTodos);
        }

        // If not in cache, get from database
        var todos = await db.Todos
            .Where(t => t.CreatedBy == userId && !t.IsDeleted)
            .ToListAsync();

        // Store in cache
        cacheService.SetUserTodos(userId, todos);

        return TypedResults.Ok(todos);
    }

    private static async Task<IResult> GetTodo(int id, ClaimsPrincipal user, TodoDb db, ITodoCacheService cacheService)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Try cache first
        var cachedTodos = await cacheService.GetUserTodosAsync(userId);
        if (cachedTodos != null)
        {
            var cachedTodo = cachedTodos.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (cachedTodo != null)
            {
                return TypedResults.Ok(cachedTodo);
            }
        }

        // Fall back to database
        var todo = await db.Todos
            .Where(t => t.Id == id && !t.IsDeleted && t.CreatedBy == userId)
            .FirstOrDefaultAsync();

        if (todo is null)
        {
            return TypedResults.NotFound();
        }

        // If user's todos are cached but this specific todo wasn't in cache, add it
        if (cachedTodos != null)
        {
            cacheService.AddTodo(userId, todo);
        }

        return TypedResults.Ok(todo);
    }

    private static async Task<IResult> CreateTodo(CreateTodoRequest request, ClaimsPrincipal user, TodoDb db, ITodoCacheService cacheService)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var todo = new Todo
        {
            Title = request.Title,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsComplete = false,
            IsDeleted = false
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        // Add to cache if user's todos are cached
        cacheService.AddTodo(userId, todo);

        return TypedResults.Created($"/todos/{todo.Id}", todo);
    }

    private static async Task<IResult> UpdateTodo(int id, UpdateTodoRequest request, ClaimsPrincipal user, TodoDb db, ITodoCacheService cacheService)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var todo = await db.Todos
            .Where(t => t.Id == id && !t.IsDeleted && t.CreatedBy == userId)
            .FirstOrDefaultAsync();

        if (todo is null) return TypedResults.NotFound();

        if (request.Title is not null)
            todo.Title = request.Title;

        if (request.IsComplete.HasValue)
            todo.IsComplete = request.IsComplete.Value;

        todo.ModifiedBy = userId;
        todo.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Update cache for the owner of the todo
        cacheService.UpdateTodo(userId, todo);

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteTodo(int id, ClaimsPrincipal user, TodoDb db, ITodoCacheService cacheService)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var todo = await db.Todos
            .Where(t => t.Id == id && !t.IsDeleted && t.CreatedBy == userId)
            .FirstOrDefaultAsync();

        if (todo is null) return TypedResults.NotFound();

        // Soft delete
        todo.IsDeleted = true;
        todo.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Remove from cache
        cacheService.RemoveTodo(userId, id);

        return TypedResults.NoContent();
    }
}
