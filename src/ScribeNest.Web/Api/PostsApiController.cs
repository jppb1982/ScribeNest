using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScribeNest.Infrastructure.Data;
using ScribeNest.Web.Api.Dtos;

namespace ScribeNest.Web.Controllers;

[ApiController]
[Route("api/posts")] // /api/posts
public class PostsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public PostsApiController(AppDbContext db) => _db = db;

    // GET /api/posts?q=&categoryId=&page=1&pageSize=5
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostListItemDto>>> Get(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0 || pageSize > 50) pageSize = 5;

        var query = _db.Posts.AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.PublishedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Content.ToLower().Contains(term) ||
                p.Category!.Name.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListItemDto(
                p.Id, p.Title, p.Slug, p.Category!.Name, p.PublishedAt))
            .ToListAsync();

        return Ok(new PagedResult<PostListItemDto>(items, total));
    }

    // GET /api/posts/123
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDetailDto>> GetById(int id)
    {
        var post = await _db.Posts.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null) return NotFound();

        var dto = new PostDetailDto(
            post.Id, post.Title, post.Slug, post.Content,
            post.Category!.Name, post.PublishedAt);

        return Ok(dto);
    }
}
