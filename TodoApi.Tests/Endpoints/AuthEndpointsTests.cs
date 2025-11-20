using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Tests.Helpers;

namespace TodoApi.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private readonly TodoApiFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(TodoApiFactory factory)
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
    public async Task Signup_WithValidEmail_ShouldReturnSuccessMessage()
    {
        // Arrange
        var request = new SignupRequest("newuser@example.com");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/signup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("check your email");

        // Verify user was created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        user.Should().NotBeNull();
        user!.IsEmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();
        user.EmailVerificationTokenExpires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Signup_WithInvalidEmail_ShouldNotCreateUser()
    {
        // Arrange
        var request = new SignupRequest("invalidemail");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/signup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Returns success to not reveal validation

        // Verify user was NOT created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "invalidemail");
        user.Should().BeNull();
    }

    [Fact]
    public async Task Signup_WithExistingEmail_ShouldReturnSuccessWithoutRevealingExistence()
    {
        // Arrange
        var existingUser = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "existing@example.com");
        var request = new SignupRequest("existing@example.com");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/signup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Message.Should().Contain("check your email");
    }

    [Fact]
    public async Task VerifyAccount_WithValidToken_ShouldVerifyUserAndReturnToken()
    {
        // Arrange
        var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").Replace("=", "");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = new User
        {
            Email = "verify@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            PasswordHash = string.Empty,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24),
            IsEmailVerified = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new VerifyAccountRequest(
            verificationToken,
            "John",
            "Doe",
            "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("verify@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");

        // Verify user was updated
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TodoDb>();
        var verifiedUser = await verifyDb.Users.FirstOrDefaultAsync(u => u.Email == "verify@example.com");
        verifiedUser.Should().NotBeNull();
        verifiedUser!.IsEmailVerified.Should().BeTrue();
        verifiedUser.FirstName.Should().Be("John");
        verifiedUser.LastName.Should().Be("Doe");
        verifiedUser.PasswordHash.Should().NotBeNullOrEmpty();
        verifiedUser.EmailVerificationToken.Should().BeNull();
    }

    [Fact]
    public async Task VerifyAccount_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new VerifyAccountRequest(
            "invalid-token",
            "John",
            "Doe",
            "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyAccount_WithExpiredToken_ShouldReturnBadRequest()
    {
        // Arrange
        var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").Replace("=", "");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = new User
        {
            Email = "expired@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            PasswordHash = string.Empty,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(-1), // Expired
            IsEmailVerified = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new VerifyAccountRequest(
            verificationToken,
            "John",
            "Doe",
            "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyAccount_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").Replace("=", "");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = new User
        {
            Email = "test@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            PasswordHash = string.Empty,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24),
            IsEmailVerified = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new VerifyAccountRequest(
            verificationToken,
            "John",
            "Doe",
            "short" // Less than 8 characters
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/verify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Message.Should().Contain("8 characters");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "login@example.com", "Password123!");
        var request = new LoginRequest("login@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("login@example.com");
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("User");

        // Verify last login was updated
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var updatedUser = await db.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "user@example.com", "CorrectPassword");
        var request = new LoginRequest("user@example.com", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnverifiedEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();

        var user = new User
        {
            Email = "unverified@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = string.Empty,
            IsEmailVerified = false
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new LoginRequest("unverified@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendVerification_WithUnverifiedEmail_ShouldReturnSuccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
        var user = new User
        {
            Email = "resend@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            PasswordHash = string.Empty,
            IsEmailVerified = false,
            EmailVerificationToken = "old-token",
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(1)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var userId = user.Id;

        // Act
        var response = await _client.PostAsync($"/auth/resend-verification?email={user.Email}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify token was regenerated
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TodoDb>();
        var updatedUser = await verifyDb.Users.FindAsync(userId);
        updatedUser!.EmailVerificationToken.Should().NotBe("old-token");
        updatedUser.EmailVerificationTokenExpires.Should().BeAfter(DateTime.UtcNow.AddHours(23));
    }

    [Fact]
    public async Task ResendVerification_WithNonExistentEmail_ShouldReturnSuccessWithoutRevealing()
    {
        // Arrange & Act
        var response = await _client.PostAsync("/auth/resend-verification?email=nonexistent@example.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Message.Should().Contain("If the email exists");
    }

    [Fact]
    public async Task ResendVerification_WithVerifiedEmail_ShouldReturnSuccessWithoutRevealing()
    {
        // Arrange
        await TestHelpers.CreateVerifiedUserAsync(_factory.Services, "verified@example.com");

        // Act
        var response = await _client.PostAsync("/auth/resend-verification?email=verified@example.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
