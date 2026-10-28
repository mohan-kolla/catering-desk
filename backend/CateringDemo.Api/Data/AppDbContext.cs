using CateringDemo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringDemo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();
    public DbSet<CateringOrder> CateringOrders => Set<CateringOrder>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
}
