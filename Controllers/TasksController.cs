using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    private string? GetUserId()
    {
        // Tenta obter o identificador do usuário dos claims. O token inclui "sub" com o Id.
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> List()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();
        var items = await _db.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> Get(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (task.UserId != userId) return Forbid();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem input)
    {
        input.Id = 0;
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = null;
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();
        // Sempre vincula a tarefa ao usuário autenticado, ignorando qualquer UserId vindo da requisição.
        input.UserId = userId;
        _db.Tasks.Add(input);
        await _db.SaveChangesAsync();
        return Created($"/api/tasks/{input.Id}", input);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItem>> Update(int id, [FromBody] TaskItem update)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (task.UserId != userId) return Forbid();
        task.Title = update.Title;
        task.Description = update.Description;
        task.Status = update.Status;
        task.UpdatedAt = DateTime.UtcNow;
        // Nunca permite alterar o proprietário da tarefa.
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (task.UserId != userId) return Forbid();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}