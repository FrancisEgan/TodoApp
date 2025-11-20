namespace TodoApi.Models.DTOs;

public record SetPasswordRequest(
    string Token,
    string Password
);
