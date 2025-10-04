using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Phone))
            return BadRequest("Campos obrigatórios ausentes");

        var exists = await _db.Users.AnyAsync(u => u.Username == req.Username || u.Email == req.Email);
        if (exists) return Conflict("Usuário ou email já existente");

        Security.CreatePasswordHash(req.Password, out var hash, out var salt);
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            Phone = req.Phone,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var resp = new UserResponse(user.Id, user.Username, user.Email, user.Phone, user.CreatedAt, user.UpdatedAt);
        return Created($"/api/users/{user.Id}", resp);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null) return Unauthorized();
        var ok = Security.VerifyPassword(req.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok) return Unauthorized();
        var resp = new UserResponse(user.Id, user.Username, user.Email, user.Phone, user.CreatedAt, user.UpdatedAt);
        return Ok(resp);
    }
}