using CateringDemo.Api.Models;

namespace CateringDemo.Api.Services;

public static class ShortfallCalculator
{
    // Totals ingredient needs across every dish in an order, then compares them to stock.
    // Expects each MenuItem's Recipe and each RecipeLine.Ingredient to be loaded.
    // Prices come from PriceResolver; any ingredient missing from it uses its estimated CostPerUnit.
    public static OrderCheckResult Calculate(
        IEnumerable<(MenuItem Item, int Servings)> lines,
        IReadOnlyDictionary<int, ResolvedPrice>? prices = null,
        string region = BlsCatalog.NationalRegion)
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

        ResolvedPrice PriceFor(Ingredient i) =>
            prices is not null && prices.TryGetValue(i.Id, out var p)
                ? p
                : new ResolvedPrice(i.CostPerUnit, PriceSources.Estimated, null);

        var priced = needs.Values
            .Select(n => (n.Ingredient, n.Quantity, Price: PriceFor(n.Ingredient)))
            .ToList();

        var foodCost = priced.Sum(n => n.Price.PricePerUnit * (decimal)n.Quantity);

        var result = priced
            .Select(n => new ShortfallLine(
                n.Ingredient.Id,
                n.Ingredient.Name,
                n.Ingredient.Unit,
                Math.Round(n.Quantity, 2),
                Math.Round(n.Ingredient.QuantityOnHand, 2),
                Math.Round(Math.Max(0, n.Quantity - n.Ingredient.QuantityOnHand), 2),
                Math.Round(n.Price.PricePerUnit, 2),
                n.Price.Source))
            .OrderByDescending(l => l.Short)
            .ThenBy(l => l.Ingredient)
            .ToList();

        var pricePeriod = priced
            .Select(n => n.Price.Period)
            .Where(period => period is not null)
            .OrderDescending()
            .FirstOrDefault();

        return new OrderCheckResult(
            Math.Round(foodCost, 2),
            Math.Round(revenue, 2),
            result.All(l => l.Short == 0),
            result,
            region,
            pricePeriod);
    }
}
