using System.ComponentModel.DataAnnotations;

namespace GParents.Shared.DTOs;

public class GroceryCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<GroceryItemDto> Items { get; set; } = new();
}

public class GroceryItemDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsChecked { get; set; }

    [StringLength(50)]
    public string? Quantity { get; set; }

    public DateTime? CheckedAt { get; set; }
}

public class CreateGroceryItemRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [StringLength(50)]
    public string? Quantity { get; set; }
}

public class UpdateGroceryItemRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    public int? CategoryId { get; set; }
    public bool? IsChecked { get; set; }

    [StringLength(50)]
    public string? Quantity { get; set; }
}
