namespace TaskManager.Api.Models;

public record RegisterRequest(string Username, string Password, string Email, string Phone);
public record LoginRequest(string Username, string Password);
public record EditUserRequest(string? Email, string? Phone, string? Password);
public record UserResponse(int Id, string Username, string Email, string Phone, DateTime CreatedAt, DateTime? UpdatedAt);