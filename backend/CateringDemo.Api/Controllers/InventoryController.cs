using CateringDemo.Api.Data;
using CateringDemo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringDemo.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Ingredient>> GetAll() =>
        await db.Ingredients.OrderBy(i => i.Name).ToListAsync();

    [HttpGet("low-stock")]
    public async Task<List<Ingredient>> GetLowStock() =>
        await db.Ingredients
            .Where(i => i.QuantityOnHand <= i.ReorderLevel)
            .OrderBy(i => i.Name)
            .ToListAsync();

    [HttpPatch("{id:int}/restock")]
    public async Task<ActionResult<Ingredient>> Restock(int id, RestockRequest request)
    {
        var ingredient = await db.Ingredients.FindAsync(id);
        if (ingredient is null) return NotFound();

        ingredient.QuantityOnHand = Math.Round(ingredient.QuantityOnHand + request.Amount, 2);
        await db.SaveChangesAsync();
        return ingredient;
    }
}
