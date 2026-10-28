using CateringDemo.Api.Models;

namespace CateringDemo.Api.Services;

public static class PriceSources
{
    public const string Regional = "regional";
    public const string National = "national";
    public const string Estimated = "estimated";
}

public record ResolvedPrice(decimal PricePerUnit, string Source, string? Period);

public static class PriceResolver
{
    // Picks a price for each ingredient: the region's BLS price first, then the
    // U.S. average, then the app's generated estimate when BLS has no data.
    public static Dictionary<int, ResolvedPrice> Resolve(
        IEnumerable<Ingredient> ingredients, MarketPriceCache cache, string region)
    {
        var resolved = new Dictionary<int, ResolvedPrice>();

        foreach (var ingredient in ingredients)
        {
            resolved[ingredient.Id] = ResolveOne(ingredient, cache, region);
        }

        return resolved;
    }

    private static ResolvedPrice ResolveOne(Ingredient ingredient, MarketPriceCache cache, string region)
    {
        if (ingredient.BlsItemCode is { } item)
        {
            if (region != BlsCatalog.NationalRegion && cache.Get(region, item) is { } regional)
            {
                return new ResolvedPrice(regional.PricePerUnit, PriceSources.Regional, regional.Period);
            }

            if (cache.Get(BlsCatalog.NationalRegion, item) is { } national)
            {
                return new ResolvedPrice(national.PricePerUnit, PriceSources.National, national.Period);
            }
        }

        return new ResolvedPrice(ingredient.CostPerUnit, PriceSources.Estimated, null);
    }
}
