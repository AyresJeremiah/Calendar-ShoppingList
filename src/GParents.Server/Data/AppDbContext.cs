using GParents.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace GParents.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<EventPerson> EventPeople => Set<EventPerson>();
    public DbSet<GroceryCategory> GroceryCategories => Set<GroceryCategory>();
    public DbSet<GroceryItem> GroceryItems => Set<GroceryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
        });

        // Person
        modelBuilder.Entity<Person>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(50).IsRequired();
            e.Property(p => p.Color).HasMaxLength(7).IsRequired();
            e.HasOne(p => p.User).WithMany(u => u.People).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // CalendarEvent
        modelBuilder.Entity<CalendarEvent>(e =>
        {
            e.Property(ev => ev.Title).HasMaxLength(200).IsRequired();
            e.Property(ev => ev.Description).HasMaxLength(500);
            e.Property(ev => ev.RecurrenceType).HasConversion<string>().HasMaxLength(10);
            e.HasOne(ev => ev.User).WithMany(u => u.Events).HasForeignKey(ev => ev.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // EventPerson (composite key join table)
        modelBuilder.Entity<EventPerson>(e =>
        {
            e.HasKey(ep => new { ep.EventId, ep.PersonId });
            e.HasOne(ep => ep.Event).WithMany(ev => ev.EventPeople).HasForeignKey(ep => ep.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ep => ep.Person).WithMany(p => p.EventPeople).HasForeignKey(ep => ep.PersonId).OnDelete(DeleteBehavior.Cascade);
        });

        // GroceryCategory
        modelBuilder.Entity<GroceryCategory>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(50).IsRequired();
        });

        // GroceryItem
        modelBuilder.Entity<GroceryItem>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(200).IsRequired();
            e.Property(i => i.Quantity).HasMaxLength(50);
            e.HasOne(i => i.User).WithMany(u => u.GroceryItems).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Category).WithMany(c => c.Items).HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed grocery categories
        modelBuilder.Entity<GroceryCategory>().HasData(
            new GroceryCategory { Id = 1, Name = "Produce", SortOrder = 1, IsDefault = true },
            new GroceryCategory { Id = 2, Name = "Dairy", SortOrder = 2, IsDefault = true },
            new GroceryCategory { Id = 3, Name = "Meat & Seafood", SortOrder = 3, IsDefault = true },
            new GroceryCategory { Id = 4, Name = "Bakery", SortOrder = 4, IsDefault = true },
            new GroceryCategory { Id = 5, Name = "Frozen", SortOrder = 5, IsDefault = true },
            new GroceryCategory { Id = 6, Name = "Pantry", SortOrder = 6, IsDefault = true },
            new GroceryCategory { Id = 7, Name = "Beverages", SortOrder = 7, IsDefault = true },
            new GroceryCategory { Id = 8, Name = "Snacks", SortOrder = 8, IsDefault = true },
            new GroceryCategory { Id = 9, Name = "Household", SortOrder = 9, IsDefault = true },
            new GroceryCategory { Id = 10, Name = "Other", SortOrder = 10, IsDefault = true }
        );
    }
}
