using System.ComponentModel.DataAnnotations;

namespace CateringDemo.Api.Models;

public record OrderLineRequest(int MenuItemId, [Range(1, 2000)] int Servings);

// Region is a BLS area code (see Services/BlsCatalog.cs). Defaults to the U.S. average.
public record CheckOrderRequest([MinLength(1)] List<OrderLineRequest> Lines, string? Region = null);

public record CreateOrderRequest(
    [Required, StringLength(100)] string CustomerName,
    DateTime EventDate,
    [Range(1, 2000)] int Headcount,
    [MinLength(1)] List<OrderLineRequest> Lines,
    string? Region = null);

public record RestockRequest([Range(0.1, 10000)] double Amount);

public record OrderLineDto(string MenuItem, int Servings);

public record OrderSummaryDto(
    int Id, string CustomerName, DateTime EventDate, int Headcount, string Status,
    List<OrderLineDto> Lines);

public record ShortfallLine(
    int IngredientId, string Ingredient, string Unit,
    double Needed, double OnHand, double Short,
    decimal UnitPrice, string PriceSource);

// PricePeriod is the most recent BLS month used, or null if every price was estimated.
public record OrderCheckResult(
    decimal FoodCost, decimal Revenue, bool CanFulfill, List<ShortfallLine> Lines,
    string Region, string? PricePeriod);

public record PriceDto(
    int IngredientId, string Ingredient, string Unit,
    decimal PricePerUnit, string Source, string? Period, string? BlsItem);

public record PriceListDto(
    string Region, string RegionName, DateTime? LastUpdatedUtc, List<PriceDto> Prices);

public record RegionDto(string Code, string Name);
