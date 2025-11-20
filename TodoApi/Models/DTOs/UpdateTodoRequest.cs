namespace TodoApi.Models.DTOs;

public record UpdateTodoRequest(
    string? Title,
    bool? IsComplete
);
