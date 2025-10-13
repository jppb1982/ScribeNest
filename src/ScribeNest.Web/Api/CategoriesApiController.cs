using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScribeNest.Infrastructure.Data;
using ScribeNest.Web.Api.Dtos;

namespace ScribeNest.Web.Api;

[ApiController]
[Route("api/categories")] // <-- ruta fija: /api/categories
public class CategoriesApiController(AppDbContext db) : ControllerBase
{
    // GET /api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
    {
        var cats = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();

        return Ok(cats);
    }
}
