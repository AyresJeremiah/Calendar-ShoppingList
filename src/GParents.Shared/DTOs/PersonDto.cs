using System.ComponentModel.DataAnnotations;

namespace GParents.Shared.DTOs;

public class PersonDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string Color { get; set; } = "#4A6FA5";

    public int SortOrder { get; set; }
}
