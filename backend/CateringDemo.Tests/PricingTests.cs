using CateringDemo.Api.Models;
using CateringDemo.Api.Services;

namespace CateringDemo.Tests;

public class BlsClientParseTests
{
    private const string SampleResponse = """
        {
          "status": "REQUEST_SUCCEEDED",
          "message": [],
          "Results": {
            "series": [
              {
                "seriesID": "APU0300706212",
                "data": [
                  { "year": "2026", "period": "M09", "periodName": "September", "value": "-" },
                  { "year": "2026", "period": "M08", "periodName": "August", "value": "2.104" },
                  { "year": "2025", "period": "M13", "periodName": "Annual", "value": "9.999" },
                  { "year": "2025", "period": "M12", "periodName": "December", "value": "1.950" }
                ]
              },
              { "seriesID": "APU0000706212", "data": [] }
            ]
          }
        }
        """;

    [Fact]
    public void TakesNewestMonth_SkippingMissingValuesAndAnnualAverages()
    {
        var prices = BlsClient.ParseLatest(SampleResponse);

        var price = prices["APU0300706212"];
        Assert.Equal(2.104m, price.PricePerUnit);
        Assert.Equal("2026-08", price.Period);
    }

    [Fact]
    public void LeavesOutSeriesWithNoData()
    {
        var prices = BlsClient.ParseLatest(SampleResponse);

        Assert.False(prices.ContainsKey("APU0000706212"));
    }

    [Fact]
    public void Throws_WhenBlsReportsFailure()
    {
        const string failed = """{ "status": "REQUEST_NOT_PROCESSED", "message": ["Daily threshold reached"] }""";

        Assert.Throws<InvalidOperationException>(() => BlsClient.ParseLatest(failed));
    }
}

public class PriceResolverTests
{
    private static Ingredient Chicken() =>
        new() { Id = 1, Name = "Chicken thigh", Unit = "lb", CostPerUnit = 3.20m, BlsItemCode = "706212" };

    private static MarketPriceCache CacheWith(params (string Region, decimal Price)[] prices)
    {
        var cache = new MarketPriceCache();
        cache.Update(
            prices.ToDictionary(p => BlsCatalog.SeriesId(p.Region, "706212"), p => new MarketPrice(p.Price, "2026-08")),
            DateTime.UtcNow);
        return cache;
    }

    [Fact]
    public void UsesRegionalPrice_WhenAvailable()
    {
        var cache = CacheWith(("0300", 2.10m), ("0000", 2.40m));

        var price = PriceResolver.Resolve(new[] { Chicken() }, cache, "0300")[1];

        Assert.Equal(2.10m, price.PricePerUnit);
        Assert.Equal(PriceSources.Regional, price.Source);
    }

    [Fact]
    public void FallsBackToNationalPrice_WhenRegionHasNoData()
    {
        var cache = CacheWith(("0000", 2.40m));

        var price = PriceResolver.Resolve(new[] { Chicken() }, cache, "0300")[1];

        Assert.Equal(2.40m, price.PricePerUnit);
        Assert.Equal(PriceSources.National, price.Source);
    }

    [Fact]
    public void FallsBackToEstimate_WhenNoBlsData()
    {
        var price = PriceResolver.Resolve(new[] { Chicken() }, new MarketPriceCache(), "0300")[1];

        Assert.Equal(3.20m, price.PricePerUnit);
        Assert.Equal(PriceSources.Estimated, price.Source);
        Assert.Null(price.Period);
    }

    [Fact]
    public void UsesEstimate_ForIngredientsWithoutABlsItem()
    {
        var oil = new Ingredient { Id = 2, Name = "Olive oil", Unit = "qt", CostPerUnit = 6.50m, BlsItemCode = null };

        var price = PriceResolver.Resolve(new[] { oil }, CacheWith(("0300", 2.10m)), "0300")[2];

        Assert.Equal(PriceSources.Estimated, price.Source);
    }
}

public class ShortfallPricingTests
{
    [Fact]
    public void UsesResolvedPrices_ForFoodCost()
    {
        var rice = new Ingredient { Id = 1, Name = "Rice", Unit = "lb", QuantityOnHand = 100, CostPerUnit = 2m };
        var dish = new MenuItem
        {
            Id = 1,
            Name = "Fried rice",
            PricePerServing = 5m,
            Recipe = { new RecipeLine { IngredientId = 1, Ingredient = rice, QuantityPerServing = 0.5 } }
        };
        var prices = new Dictionary<int, ResolvedPrice>
        {
            [1] = new(1.50m, PriceSources.Regional, "2026-08")
        };

        var result = ShortfallCalculator.Calculate(new[] { (dish, 40) }, prices, "0300");

        Assert.Equal(30m, result.FoodCost);          // 20 lb x $1.50 market price, not the $2 estimate
        Assert.Equal("0300", result.Region);
        Assert.Equal("2026-08", result.PricePeriod);
        Assert.Equal(PriceSources.Regional, result.Lines.Single().PriceSource);
    }
}
