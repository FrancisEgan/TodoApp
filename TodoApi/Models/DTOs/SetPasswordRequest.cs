namespace TodoApi.Models.DTOs;

public record SetPasswordRequest(
    string Token,
    string Password
);

public record VerifyAccountRequest(
    string Token,
    string FirstName,
    string LastName,
    string Password
);
