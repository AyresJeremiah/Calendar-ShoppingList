namespace GParents.Server.Entities;

public class GroceryCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }

    public List<GroceryItem> Items { get; set; } = new();
}
