using System.ComponentModel.DataAnnotations;

namespace CateringDemo.Api.Models;

public record OrderLineRequest(int MenuItemId, [Range(1, 2000)] int Servings);

public record CheckOrderRequest([MinLength(1)] List<OrderLineRequest> Lines);

public record CreateOrderRequest(
    [Required, StringLength(100)] string CustomerName,
    DateTime EventDate,
    [Range(1, 2000)] int Headcount,
    [MinLength(1)] List<OrderLineRequest> Lines);

public record RestockRequest([Range(0.1, 10000)] double Amount);

public record OrderLineDto(string MenuItem, int Servings);

public record OrderSummaryDto(
    int Id, string CustomerName, DateTime EventDate, int Headcount, string Status,
    List<OrderLineDto> Lines);

public record ShortfallLine(
    int IngredientId, string Ingredient, string Unit,
    double Needed, double OnHand, double Short);

public record OrderCheckResult(
    decimal FoodCost, decimal Revenue, bool CanFulfill, List<ShortfallLine> Lines);
