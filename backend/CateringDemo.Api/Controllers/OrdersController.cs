using CateringDemo.Api.Data;
using CateringDemo.Api.Models;
using CateringDemo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringDemo.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(AppDbContext db, MarketPriceCache priceCache) : ControllerBase
{
    [HttpGet]
    public async Task<List<OrderSummaryDto>> GetAll() =>
        await db.CateringOrders
            .OrderBy(o => o.EventDate)
            .Select(o => new OrderSummaryDto(
                o.Id, o.CustomerName, o.EventDate, o.Headcount, o.Status,
                o.Lines.Select(l => new OrderLineDto(l.MenuItem.Name, l.Servings)).ToList()))
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderSummaryDto>> GetById(int id)
    {
        var order = await db.CateringOrders
            .Where(o => o.Id == id)
            .Select(o => new OrderSummaryDto(
                o.Id, o.CustomerName, o.EventDate, o.Headcount, o.Status,
                o.Lines.Select(l => new OrderLineDto(l.MenuItem.Name, l.Servings)).ToList()))
            .FirstOrDefaultAsync();

        if (order is null) return NotFound();
        return order;
    }

    // Checks an order against current stock without saving it.
    [HttpPost("check")]
    public async Task<ActionResult<OrderCheckResult>> Check(CheckOrderRequest request)
    {
        var region = request.Region ?? BlsCatalog.NationalRegion;
        if (!BlsCatalog.IsValidRegion(region)) return BadRequest($"Unknown region '{region}'.");

        var lines = await ResolveLinesAsync(request.Lines);
        if (lines is null) return BadRequest("One or more menu items do not exist.");
        return Price(lines, region);
    }

    [HttpPost]
    public async Task<ActionResult<OrderCheckResult>> Create(CreateOrderRequest request)
    {
        var region = request.Region ?? BlsCatalog.NationalRegion;
        if (!BlsCatalog.IsValidRegion(region)) return BadRequest($"Unknown region '{region}'.");

        var lines = await ResolveLinesAsync(request.Lines);
        if (lines is null) return BadRequest("One or more menu items do not exist.");

        var order = new CateringOrder
        {
            CustomerName = request.CustomerName.Trim(),
            EventDate = request.EventDate.Date,
            Headcount = request.Headcount,
            Lines = request.Lines
                .Select(l => new OrderLine { MenuItemId = l.MenuItemId, Servings = l.Servings })
                .ToList()
        };

        db.CateringOrders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, Price(lines, region));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.CateringOrders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        db.CateringOrders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private OrderCheckResult Price(List<(MenuItem Item, int Servings)> lines, string region)
    {
        var ingredients = lines
            .SelectMany(l => l.Item.Recipe.Select(r => r.Ingredient))
            .DistinctBy(i => i.Id);
        var prices = PriceResolver.Resolve(ingredients, priceCache, region);
        return ShortfallCalculator.Calculate(lines, prices, region);
    }

    private async Task<List<(MenuItem Item, int Servings)>?> ResolveLinesAsync(List<OrderLineRequest> requested)
    {
        var ids = requested.Select(l => l.MenuItemId).Distinct().ToList();
        var items = await db.MenuItems
            .Include(m => m.Recipe).ThenInclude(r => r.Ingredient)
            .Where(m => ids.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        if (items.Count != ids.Count) return null;
        return requested.Select(l => (items[l.MenuItemId], l.Servings)).ToList();
    }
}
