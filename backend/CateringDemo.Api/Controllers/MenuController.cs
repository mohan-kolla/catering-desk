using CateringDemo.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringDemo.Api.Controllers;

[ApiController]
[Route("api/menu")]
public class MenuController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.MenuItems
        .OrderBy(m => m.Category).ThenBy(m => m.Name)
        .Select(m => new { m.Id, m.Name, m.Category, m.PricePerServing })
        .ToListAsync());
}
