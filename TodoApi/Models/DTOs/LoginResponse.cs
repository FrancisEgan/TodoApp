namespace TodoApi.Models.DTOs;

public record LoginResponse(
    string Token,
    int UserId,
    string Email,
    string FirstName,
    string LastName
);
