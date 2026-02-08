using System.Security.Claims;
using GParents.Server.Data;
using GParents.Server.Entities;
using GParents.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GParents.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PeopleController : ControllerBase
{
    private readonly AppDbContext _db;

    public PeopleController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<PersonDto>>> GetAll()
    {
        var people = await _db.People
            .Where(p => p.UserId == GetUserId())
            .OrderBy(p => p.SortOrder)
            .Select(p => new PersonDto
            {
                Id = p.Id,
                Name = p.Name,
                Color = p.Color,
                SortOrder = p.SortOrder
            })
            .ToListAsync();

        return Ok(people);
    }

    [HttpPost]
    public async Task<ActionResult<PersonDto>> Create(PersonDto dto)
    {
        var userId = GetUserId();
        var maxSort = await _db.People.Where(p => p.UserId == userId).MaxAsync(p => (int?)p.SortOrder) ?? 0;

        var person = new Person
        {
            UserId = userId,
            Name = dto.Name,
            Color = dto.Color,
            SortOrder = maxSort + 1
        };

        _db.People.Add(person);
        await _db.SaveChangesAsync();

        dto.Id = person.Id;
        dto.SortOrder = person.SortOrder;
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PersonDto>> Update(int id, PersonDto dto)
    {
        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == id && p.UserId == GetUserId());
        if (person == null) return NotFound();

        person.Name = dto.Name;
        person.Color = dto.Color;
        person.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();

        dto.Id = person.Id;
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == id && p.UserId == GetUserId());
        if (person == null) return NotFound();

        _db.People.Remove(person);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
