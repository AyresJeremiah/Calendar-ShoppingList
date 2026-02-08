using System.ComponentModel.DataAnnotations;

namespace GParents.Shared.DTOs;

public class CalendarEventDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string RecurrenceType { get; set; } = "None";
    public DateTime? RecurrenceEndDate { get; set; }
    public List<int> PersonIds { get; set; } = new();
    public List<PersonDto> People { get; set; } = new();
}

public class CalendarEventOccurrenceDto
{
    public int SourceEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string RecurrenceType { get; set; } = "None";
    public List<PersonDto> People { get; set; } = new();
}
