namespace TodoApi.Models.DTOs;

public record SignupRequest(
    string FirstName,
    string LastName,
    string Email
);
