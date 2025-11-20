using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost("/signup", Signup)
            .WithName("Signup");

        auth.MapPost("/set-password", SetPassword)
            .WithName("SetPassword");

        auth.MapPost("/login", Login)
            .WithName("Login");

        auth.MapPost("/resend-verification", ResendVerification)
            .WithName("ResendVerification");
    }

    private static async Task<IResult> Signup(
        SignupRequest request,
        TodoDb db,
        IEmailService emailService)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return TypedResults.BadRequest(new AuthResponse("Invalid email address"));
        }

        // Check if user already exists
        if (await db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return TypedResults.BadRequest(new AuthResponse("Email already registered"));
        }

        // Generate verification token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            EmailVerificationToken = token,
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24),
            IsEmailVerified = false,
            PasswordHash = string.Empty // Will be set when they verify email
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Send verification email
        await emailService.SendVerificationEmailAsync(user.Email, token);

        return TypedResults.Ok(new AuthResponse(
            "Verification email sent. Please check your inbox.",
            user.Id
        ));
    }

    private static async Task<IResult> SetPassword(
        SetPasswordRequest request,
        TodoDb db,
        PasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        // Find user by token
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.EmailVerificationToken == request.Token &&
            u.EmailVerificationTokenExpires > DateTime.UtcNow
        );

        if (user == null)
        {
            return TypedResults.BadRequest(new AuthResponse("Invalid or expired token"));
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return TypedResults.BadRequest(new AuthResponse("Password must be at least 8 characters"));
        }

        // Hash and set password
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpires = null;
        user.LastLoginAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Generate JWT token to automatically log them in
        var token = tokenService.GenerateToken(user);

        return TypedResults.Ok(new LoginResponse(
            token,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        ));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        TodoDb db,
        PasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !user.IsEmailVerified)
        {
            return TypedResults.Unauthorized();
        }

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            return TypedResults.Unauthorized();
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Generate JWT token
        var token = tokenService.GenerateToken(user);

        return TypedResults.Ok(new LoginResponse(
            token,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        ));
    }

    private static async Task<IResult> ResendVerification(
        string email,
        TodoDb db,
        IEmailService emailService)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == email &&
            !u.IsEmailVerified
        );

        if (user == null)
        {
            // Don't reveal if email exists
            return TypedResults.Ok(new AuthResponse("If the email exists, a verification link has been sent."));
        }

        // Generate new token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        user.EmailVerificationToken = token;
        user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

        await db.SaveChangesAsync();
        await emailService.SendVerificationEmailAsync(user.Email, token);

        return TypedResults.Ok(new AuthResponse("If the email exists, a verification link has been sent."));
    }
}
