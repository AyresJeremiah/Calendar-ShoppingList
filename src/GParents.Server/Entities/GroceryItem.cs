namespace GParents.Server.Entities;

public class GroceryItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public string? Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedAt { get; set; }

    public User User { get; set; } = null!;
    public GroceryCategory Category { get; set; } = null!;
}
