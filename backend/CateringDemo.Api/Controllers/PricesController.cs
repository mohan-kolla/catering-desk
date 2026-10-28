using CateringDemo.Api.Data;
using CateringDemo.Api.Models;
using CateringDemo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringDemo.Api.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController(AppDbContext db, MarketPriceCache cache) : ControllerBase
{
    [HttpGet("regions")]
    public IEnumerable<RegionDto> GetRegions() =>
        BlsCatalog.Regions.Select(r => new RegionDto(r.Key, r.Value));

    // Current price for every ingredient in a region, with where each price came from.
    [HttpGet]
    public async Task<ActionResult<PriceListDto>> Get([FromQuery] string region = BlsCatalog.NationalRegion)
    {
        if (!BlsCatalog.IsValidRegion(region)) return BadRequest($"Unknown region '{region}'.");

        var ingredients = await db.Ingredients.OrderBy(i => i.Name).ToListAsync();
        var resolved = PriceResolver.Resolve(ingredients, cache, region);

        var prices = ingredients.Select(i =>
        {
            var p = resolved[i.Id];
            var blsItem = i.BlsItemCode is { } code && BlsCatalog.Items.TryGetValue(code, out var name) ? name : null;
            return new PriceDto(i.Id, i.Name, i.Unit, Math.Round(p.PricePerUnit, 2), p.Source, p.Period, blsItem);
        }).ToList();

        return new PriceListDto(region, BlsCatalog.Regions[region], cache.LastUpdatedUtc, prices);
    }
}
