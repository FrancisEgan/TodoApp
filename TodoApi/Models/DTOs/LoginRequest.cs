namespace TodoApi.Models.DTOs;

public record LoginRequest(
    string Email,
    string Password
);
