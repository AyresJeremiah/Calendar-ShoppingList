namespace GParents.Server.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public List<Person> People { get; set; } = new();
    public List<CalendarEvent> Events { get; set; } = new();
    public List<GroceryItem> GroceryItems { get; set; } = new();
}
