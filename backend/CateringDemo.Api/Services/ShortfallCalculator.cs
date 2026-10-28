using CateringDemo.Api.Models;

namespace CateringDemo.Api.Services;

public static class ShortfallCalculator
{
    // Totals ingredient needs across every dish in an order, then compares them to stock.
    // Expects each MenuItem's Recipe and each RecipeLine.Ingredient to be loaded.
    public static OrderCheckResult Calculate(IEnumerable<(MenuItem Item, int Servings)> lines)
    {
        var needs = new Dictionary<int, (Ingredient Ingredient, double Quantity)>();
        decimal revenue = 0;

        foreach (var (item, servings) in lines)
        {
            revenue += item.PricePerServing * servings;

            foreach (var recipe in item.Recipe)
            {
                var ingredient = recipe.Ingredient;
                var qty = recipe.QuantityPerServing * servings;
                needs[ingredient.Id] = needs.TryGetValue(ingredient.Id, out var current)
                    ? (ingredient, current.Quantity + qty)
                    : (ingredient, qty);
            }
        }

        var foodCost = needs.Values.Sum(n => n.Ingredient.CostPerUnit * (decimal)n.Quantity);

        var result = needs.Values
            .Select(n => new ShortfallLine(
                n.Ingredient.Id,
                n.Ingredient.Name,
                n.Ingredient.Unit,
                Math.Round(n.Quantity, 2),
                Math.Round(n.Ingredient.QuantityOnHand, 2),
                Math.Round(Math.Max(0, n.Quantity - n.Ingredient.QuantityOnHand), 2)))
            .OrderByDescending(l => l.Short)
            .ThenBy(l => l.Ingredient)
            .ToList();

        return new OrderCheckResult(
            Math.Round(foodCost, 2),
            Math.Round(revenue, 2),
            result.All(l => l.Short == 0),
            result);
    }
}
