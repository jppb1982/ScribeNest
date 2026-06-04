using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScribeNest.Domain.Entities;
using ScribeNest.Infrastructure.Data;
using ScribeNest.Web.Api.Dtos;

namespace ScribeNest.Web.Api;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController(AppDbContext db, ILogger<CategoriesApiController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
    {
        var cats = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();

        return Ok(cats);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsertDto request)
    {
        var name = request.Name.Trim();

        if (await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(request.Name), "Category already exists.");
            return ValidationProblem(ModelState);
        }

        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Category {CategoryId} created", category.Id);

        return CreatedAtAction(nameof(Get), new { id = category.Id }, new CategoryDto(category.Id, category.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryUpsertDto request)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        var name = request.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError(nameof(request.Name), "Category already exists.");
            return ValidationProblem(ModelState);
        }

        category.Name = name;
        await db.SaveChangesAsync();

        logger.LogInformation("Category {CategoryId} updated", category.Id);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.Include(c => c.Posts).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        if (category.Posts.Any())
        {
            return Conflict("Cannot delete a category with associated posts.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Category {CategoryId} deleted", id);

        return NoContent();
    }
}
