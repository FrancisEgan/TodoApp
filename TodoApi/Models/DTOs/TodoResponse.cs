namespace TodoApi.Models.DTOs;

public record TodoResponse(
    int Id,
    string Title,
    bool IsComplete,
    DateTime CreatedAt
);
