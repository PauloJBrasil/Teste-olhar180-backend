using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> List()
    {
        var items = await _db.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> Get(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem input)
    {
        input.Id = 0;
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = null;
        _db.Tasks.Add(input);
        await _db.SaveChangesAsync();
        return Created($"/api/tasks/{input.Id}", input);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItem>> Update(int id, [FromBody] TaskItem update)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        task.Title = update.Title;
        task.Description = update.Description;
        task.Status = update.Status;
        task.UpdatedAt = DateTime.UtcNow;
        task.UserId = update.UserId;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}