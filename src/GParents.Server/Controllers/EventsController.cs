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
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<CalendarEventOccurrenceDto>>> GetEvents(
        [FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var userId = GetUserId();
        var events = await _db.CalendarEvents
            .Where(e => e.UserId == userId)
            .Include(e => e.EventPeople).ThenInclude(ep => ep.Person)
            .ToListAsync();

        var occurrences = new List<CalendarEventOccurrenceDto>();
        var maxEnd = end > DateTime.UtcNow.AddYears(2) ? DateTime.UtcNow.AddYears(2) : end;

        foreach (var evt in events)
        {
            var people = evt.EventPeople.Select(ep => new PersonDto
            {
                Id = ep.Person.Id,
                Name = ep.Person.Name,
                Color = ep.Person.Color,
                SortOrder = ep.Person.SortOrder
            }).ToList();

            if (evt.RecurrenceType == RecurrenceType.None)
            {
                if (evt.StartDate < end && evt.EndDate > start)
                {
                    occurrences.Add(new CalendarEventOccurrenceDto
                    {
                        SourceEventId = evt.Id,
                        Title = evt.Title,
                        Description = evt.Description,
                        StartDate = evt.StartDate,
                        EndDate = evt.EndDate,
                        IsAllDay = evt.IsAllDay,
                        RecurrenceType = evt.RecurrenceType.ToString(),
                        People = people
                    });
                }
            }
            else
            {
                var recEnd = evt.RecurrenceEndDate ?? DateTime.UtcNow.AddYears(2);
                if (recEnd > maxEnd) recEnd = maxEnd;

                var duration = evt.EndDate - evt.StartDate;
                var current = evt.StartDate;

                while (current <= recEnd)
                {
                    var occEnd = current + duration;
                    if (current < end && occEnd > start)
                    {
                        occurrences.Add(new CalendarEventOccurrenceDto
                        {
                            SourceEventId = evt.Id,
                            Title = evt.Title,
                            Description = evt.Description,
                            StartDate = current,
                            EndDate = occEnd,
                            IsAllDay = evt.IsAllDay,
                            RecurrenceType = evt.RecurrenceType.ToString(),
                            People = people
                        });
                    }

                    current = evt.RecurrenceType switch
                    {
                        RecurrenceType.Weekly => current.AddDays(7),
                        RecurrenceType.Monthly => current.AddMonths(1),
                        RecurrenceType.Yearly => current.AddYears(1),
                        _ => recEnd.AddDays(1) // break loop
                    };
                }
            }
        }

        return Ok(occurrences.OrderBy(o => o.StartDate).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CalendarEventDto>> GetEvent(int id)
    {
        var evt = await _db.CalendarEvents
            .Where(e => e.Id == id && e.UserId == GetUserId())
            .Include(e => e.EventPeople).ThenInclude(ep => ep.Person)
            .FirstOrDefaultAsync();

        if (evt == null) return NotFound();

        return Ok(new CalendarEventDto
        {
            Id = evt.Id,
            Title = evt.Title,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            IsAllDay = evt.IsAllDay,
            RecurrenceType = evt.RecurrenceType.ToString(),
            RecurrenceEndDate = evt.RecurrenceEndDate,
            PersonIds = evt.EventPeople.Select(ep => ep.PersonId).ToList(),
            People = evt.EventPeople.Select(ep => new PersonDto
            {
                Id = ep.Person.Id,
                Name = ep.Person.Name,
                Color = ep.Person.Color,
                SortOrder = ep.Person.SortOrder
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventDto>> Create(CalendarEventDto dto)
    {
        var userId = GetUserId();
        var evt = new CalendarEvent
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAllDay = dto.IsAllDay,
            RecurrenceType = Enum.Parse<RecurrenceType>(dto.RecurrenceType),
            RecurrenceEndDate = dto.RecurrenceEndDate
        };

        _db.CalendarEvents.Add(evt);
        await _db.SaveChangesAsync();

        if (dto.PersonIds.Any())
        {
            var validPersonIds = await _db.People
                .Where(p => p.UserId == userId && dto.PersonIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var personId in validPersonIds)
            {
                _db.EventPeople.Add(new EventPerson { EventId = evt.Id, PersonId = personId });
            }
            await _db.SaveChangesAsync();
        }

        dto.Id = evt.Id;
        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CalendarEventDto>> Update(int id, CalendarEventDto dto)
    {
        var userId = GetUserId();
        var evt = await _db.CalendarEvents
            .Include(e => e.EventPeople)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (evt == null) return NotFound();

        evt.Title = dto.Title;
        evt.Description = dto.Description;
        evt.StartDate = dto.StartDate;
        evt.EndDate = dto.EndDate;
        evt.IsAllDay = dto.IsAllDay;
        evt.RecurrenceType = Enum.Parse<RecurrenceType>(dto.RecurrenceType);
        evt.RecurrenceEndDate = dto.RecurrenceEndDate;

        // Update person assignments
        _db.EventPeople.RemoveRange(evt.EventPeople);

        if (dto.PersonIds.Any())
        {
            var validPersonIds = await _db.People
                .Where(p => p.UserId == userId && dto.PersonIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var personId in validPersonIds)
            {
                _db.EventPeople.Add(new EventPerson { EventId = evt.Id, PersonId = personId });
            }
        }

        await _db.SaveChangesAsync();
        dto.Id = evt.Id;
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var evt = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == GetUserId());
        if (evt == null) return NotFound();

        _db.CalendarEvents.Remove(evt);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
