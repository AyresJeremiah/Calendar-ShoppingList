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
public class GroceryController : ControllerBase
{
    private readonly AppDbContext _db;

    public GroceryController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("categories")]
    public async Task<ActionResult<List<GroceryCategoryDto>>> GetCategories()
    {
        var userId = GetUserId();
        var categories = await _db.GroceryCategories
            .OrderBy(c => c.SortOrder)
            .Select(c => new GroceryCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder,
                Items = c.Items
                    .Where(i => i.UserId == userId)
                    .OrderBy(i => i.IsChecked)
                    .ThenBy(i => i.CreatedAt)
                    .Select(i => new GroceryItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        CategoryId = i.CategoryId,
                        CategoryName = c.Name,
                        IsChecked = i.IsChecked,
                        Quantity = i.Quantity,
                        CheckedAt = i.CheckedAt
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("items")]
    public async Task<ActionResult<GroceryItemDto>> CreateItem(CreateGroceryItemRequest request)
    {
        var userId = GetUserId();
        var category = await _db.GroceryCategories.FindAsync(request.CategoryId);
        if (category == null) return BadRequest("Invalid category");

        var item = new GroceryItem
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Quantity = request.Quantity
        };

        _db.GroceryItems.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new GroceryItemDto
        {
            Id = item.Id,
            Name = item.Name,
            CategoryId = item.CategoryId,
            CategoryName = category.Name,
            IsChecked = false,
            Quantity = item.Quantity
        });
    }

    [HttpPut("items/{id}")]
    public async Task<ActionResult<GroceryItemDto>> UpdateItem(int id, UpdateGroceryItemRequest request)
    {
        var item = await _db.GroceryItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == GetUserId());

        if (item == null) return NotFound();

        if (request.Name != null) item.Name = request.Name;
        if (request.CategoryId.HasValue) item.CategoryId = request.CategoryId.Value;
        if (request.Quantity != null) item.Quantity = request.Quantity;
        if (request.IsChecked.HasValue)
        {
            item.IsChecked = request.IsChecked.Value;
            item.CheckedAt = request.IsChecked.Value ? DateTime.UtcNow : null;
        }

        await _db.SaveChangesAsync();

        return Ok(new GroceryItemDto
        {
            Id = item.Id,
            Name = item.Name,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            IsChecked = item.IsChecked,
            Quantity = item.Quantity,
            CheckedAt = item.CheckedAt
        });
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult> DeleteItem(int id)
    {
        var item = await _db.GroceryItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == GetUserId());
        if (item == null) return NotFound();

        _db.GroceryItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("items/checked")]
    public async Task<ActionResult> ClearChecked()
    {
        var userId = GetUserId();
        var checkedItems = await _db.GroceryItems
            .Where(i => i.UserId == userId && i.IsChecked)
            .ToListAsync();

        _db.GroceryItems.RemoveRange(checkedItems);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
