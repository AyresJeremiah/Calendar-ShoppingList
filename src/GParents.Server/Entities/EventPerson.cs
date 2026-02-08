namespace GParents.Server.Entities;

public class EventPerson
{
    public int EventId { get; set; }
    public int PersonId { get; set; }

    public CalendarEvent Event { get; set; } = null!;
    public Person Person { get; set; } = null!;
}
