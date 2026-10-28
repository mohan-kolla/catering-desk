using Bogus;
using CateringDemo.Api.Models;

namespace CateringDemo.Api.Data;

// Generates a fictional restaurant's data. Nothing here comes from a real business.
public static class SeedData
{
    private static readonly (string Name, string Unit, double Cost)[] IngredientPool =
    {
        ("Chicken thigh", "lb", 3.20), ("Ground beef", "lb", 4.80), ("Basmati rice", "lb", 1.10),
        ("Penne pasta", "lb", 1.40), ("Yellow onion", "lb", 0.90), ("Tomatoes", "lb", 1.60),
        ("Bell peppers", "lb", 2.10), ("Potatoes", "lb", 0.70), ("Mixed greens", "lb", 3.50),
        ("Red lentils", "lb", 1.30), ("Olive oil", "qt", 6.50), ("Heavy cream", "qt", 4.20),
        ("Parmesan", "lb", 9.00), ("Seasonal fruit", "lb", 2.40), ("Flatbread", "ea", 0.60)
    };

    private static readonly (string Name, string Category)[] MenuPool =
    {
        ("Grilled chicken platter", "Entree"), ("Beef kebab tray", "Entree"),
        ("Pasta primavera", "Entree"), ("Vegetable fried rice", "Side"),
        ("Roasted potatoes", "Side"), ("Garden salad tray", "Side"),
        ("Lentil soup", "Side"), ("Fruit platter", "Dessert")
    };

    public static void Initialize(AppDbContext db, int seed = 42)
    {
        if (db.Ingredients.Any()) return;

        Randomizer.Seed = new Random(seed); // same seed, same demo data after every reset
        var f = new Faker();

        var ingredients = IngredientPool.Select(p => new Ingredient
        {
            Name = p.Name,
            Unit = p.Unit,
            CostPerUnit = (decimal)p.Cost,
            ReorderLevel = Math.Round(f.Random.Double(10, 30)),
            QuantityOnHand = Math.Round(f.Random.Double(0, 80), 1)
        }).ToList();

        var menu = MenuPool.Select(m => new MenuItem
        {
            Name = m.Name,
            Category = m.Category,
            PricePerServing = Math.Round((decimal)f.Random.Double(6, 18), 2),
            Recipe = f.Random.ListItems(ingredients, f.Random.Int(2, 4))
                .Select(i => new RecipeLine
                {
                    Ingredient = i,
                    QuantityPerServing = Math.Round(f.Random.Double(0.05, 0.5), 2)
                }).ToList()
        }).ToList();

        var orders = new Faker<CateringOrder>()
            .RuleFor(o => o.CustomerName, x => x.Company.CompanyName())
            .RuleFor(o => o.EventDate, x => x.Date.Between(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(45)).Date)
            .RuleFor(o => o.Headcount, x => x.Random.Int(20, 150))
            .RuleFor(o => o.Status, (x, o) => o.EventDate < DateTime.Today
                ? "Completed"
                : x.PickRandom("Pending", "Confirmed"))
            .RuleFor(o => o.Lines, (x, o) => x.Random.ListItems(menu, x.Random.Int(2, 4))
                .Select(m => new OrderLine { MenuItem = m, Servings = o.Headcount })
                .ToList())
            .Generate(12);

        db.Ingredients.AddRange(ingredients);
        db.MenuItems.AddRange(menu);
        db.CateringOrders.AddRange(orders);
        db.SaveChanges();
    }
}
