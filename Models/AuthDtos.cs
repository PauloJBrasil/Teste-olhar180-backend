namespace TaskManager.Api.Models;

using System.ComponentModel.DataAnnotations;

public record RegisterRequest(
    [Required, MinLength(3)] string Username,
    [Required, MinLength(6)] string Password,
    [Required, EmailAddress] string Email,
    [Required] string Phone);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record EditUserRequest(
    [EmailAddress] string? Email,
    string? Phone,
    [MinLength(6)] string? Password);

public record UserResponse(int Id, string Username, string Email, string Phone, DateTime CreatedAt, DateTime? UpdatedAt);
public record AuthResponse(string Token, UserResponse User);