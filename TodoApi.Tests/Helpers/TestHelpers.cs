using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Tests.Helpers;

public static class TestHelpers
{
    public static async Task<User> CreateVerifiedUserAsync(IServiceProvider services, string email = "test@example.com", string password = "Test1234!")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            IsEmailVerified = true,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public static async Task<string> GetAuthTokenAsync(IServiceProvider services, User user)
    {
        using var scope = services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return tokenService.GenerateToken(user);
    }

    public static void AddAuthToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static StringContent CreateJsonContent(object obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<TodoDb> GetDbContextAsync(IServiceProvider services)
    {
        var scope = services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TodoDb>();
    }

    public static async Task ClearDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();

        // Don't use RemoveRange which causes tracking issues with InMemory DB
        // Instead, ensure a fresh database for each test by clearing all entries directly
        foreach (var entity in db.Todos.ToList())
        {
            db.Entry(entity).State = EntityState.Detached;
        }
        foreach (var entity in db.Users.ToList())
        {
            db.Entry(entity).State = EntityState.Detached;
        }

        db.Todos.RemoveRange(db.Todos.ToList());
        db.Users.RemoveRange(db.Users.ToList());
        await db.SaveChangesAsync();
    }
}
