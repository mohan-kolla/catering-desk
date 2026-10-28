using CateringDemo.Api.Models;
using CateringDemo.Api.Services;

namespace CateringDemo.Tests;

public class ShortfallCalculatorTests
{
    private static Ingredient Rice(double onHand) =>
        new() { Id = 1, Name = "Rice", Unit = "lb", QuantityOnHand = onHand, CostPerUnit = 2m };

    private static MenuItem Dish(int id, Ingredient ingredient, double perServing, decimal price = 5m) =>
        new()
        {
            Id = id,
            Name = $"Dish {id}",
            PricePerServing = price,
            Recipe = { new RecipeLine { IngredientId = ingredient.Id, Ingredient = ingredient, QuantityPerServing = perServing } }
        };

    [Fact]
    public void ReportsShortfall_WhenStockIsTooLow()
    {
        var rice = Rice(onHand: 10);
        var result = ShortfallCalculator.Calculate(new[] { (Dish(1, rice, 0.5), 40) }); // needs 20

        Assert.False(result.CanFulfill);
        Assert.Equal(10.0, result.Lines.Single().Short);
    }

    [Fact]
    public void CanFulfill_WhenStockIsSufficient()
    {
        var rice = Rice(onHand: 50);
        var result = ShortfallCalculator.Calculate(new[] { (Dish(1, rice, 0.5), 40) });

        Assert.True(result.CanFulfill);
        Assert.Equal(0.0, result.Lines.Single().Short);
    }

    [Fact]
    public void CombinesSameIngredient_AcrossDishes()
    {
        var rice = Rice(onHand: 8);
        var result = ShortfallCalculator.Calculate(new[]
        {
            (Dish(1, rice, 0.5), 10),
            (Dish(2, rice, 0.5), 10)
        }); // needs 10 total

        var line = Assert.Single(result.Lines);
        Assert.Equal(10.0, line.Needed);
        Assert.Equal(2.0, line.Short);
    }

    [Fact]
    public void CalculatesFoodCostAndRevenue()
    {
        var rice = Rice(onHand: 100);
        var result = ShortfallCalculator.Calculate(new[] { (Dish(1, rice, 0.5, price: 5m), 40) });

        Assert.Equal(40m, result.FoodCost);   // 20 lb x $2
        Assert.Equal(200m, result.Revenue);   // 40 servings x $5
    }
}
