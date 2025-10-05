using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] EditUserRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (req.Email is not null) user.Email = req.Email;
        if (req.Phone is not null) user.Phone = req.Phone;
        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            Security.CreatePasswordHash(req.Password, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var resp = new UserResponse(user.Id, user.Username, user.Email, user.Phone, user.CreatedAt, user.UpdatedAt);
        return Ok(resp);
    }
}