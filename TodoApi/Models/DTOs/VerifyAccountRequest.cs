namespace TodoApi.Models.DTOs;

public record VerifyAccountRequest(
    string Token,
    string FirstName,
    string LastName,
    string Password
);
