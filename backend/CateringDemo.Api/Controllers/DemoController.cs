using CateringDemo.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace CateringDemo.Api.Controllers;

// Lets portfolio visitors restore the generated data after experimenting.
[ApiController]
[Route("api/demo")]
public class DemoController(AppDbContext db) : ControllerBase
{
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        SeedData.Initialize(db);
        return NoContent();
    }
}
