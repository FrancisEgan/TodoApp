namespace TodoApi.Models.DTOs;

public record AuthResponse(
    string Message,
    int? UserId = null
);
