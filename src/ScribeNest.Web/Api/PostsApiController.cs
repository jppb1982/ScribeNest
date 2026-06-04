using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScribeNest.Domain.Entities;
using ScribeNest.Infrastructure.Data;
using ScribeNest.Web.Api.Dtos;
using ScribeNest.Web.Helpers;

namespace ScribeNest.Web.Api;

[ApiController]
[Route("api/posts")]
public class PostsApiController(AppDbContext db, ILogger<PostsApiController> logger) : ControllerBase
{
    private const int ExcerptLength = 140;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostListItemDto>>> Get(
        [FromQuery] string? q,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize is <= 0 or > 50 ? 5 : pageSize;

        var query = db.Posts.AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.PublishedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Content.ToLower().Contains(term) ||
                p.Tags.ToLower().Contains(term) ||
                p.Category!.Name.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.CategoryId,
                Category = p.Category!.Name,
                p.PublishedAt,
                p.Tags
            })
            .ToListAsync();

        var items = rows
            .Select(p => new PostListItemDto(
                p.Id, p.Title, p.Slug, CreateExcerpt(p.Content), p.CategoryId, p.Category, p.PublishedAt, TagParser.ToList(p.Tags)))
            .ToList();

        return Ok(new PagedResult<PostListItemDto>(items, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDetailDto>> GetById(int id)
    {
        var post = await db.Posts.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return post is null ? NotFound() : Ok(ToDetail(post));
    }

    [HttpPost]
    public async Task<ActionResult<PostDetailDto>> Create(PostUpsertDto request)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            ModelState.AddModelError(nameof(request.CategoryId), "The selected category does not exist.");
            return ValidationProblem(ModelState);
        }

        var slug = await GenerateUniqueSlugAsync(request.Title);

        var post = new Post
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Content = request.Content.Trim(),
            Tags = TagParser.Normalize(request.Tags),
            CategoryId = request.CategoryId,
            PublishedAt = DateTime.UtcNow
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        logger.LogInformation("Post {PostId} created", post.Id);

        var created = await db.Posts.AsNoTracking().Include(p => p.Category).FirstAsync(p => p.Id == post.Id);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, ToDetail(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PostUpsertDto request)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
        {
            ModelState.AddModelError(nameof(request.CategoryId), "The selected category does not exist.");
            return ValidationProblem(ModelState);
        }

        var slug = await GenerateUniqueSlugAsync(request.Title, id);

        post.Title = request.Title.Trim();
        post.Slug = slug;
        post.Content = request.Content.Trim();
        post.Tags = TagParser.Normalize(request.Tags);
        post.CategoryId = request.CategoryId;

        await db.SaveChangesAsync();
        logger.LogInformation("Post {PostId} updated", post.Id);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        logger.LogInformation("Post {PostId} deleted", id);

        return NoContent();
    }

    private static PostListItemDto ToListItem(Post p) =>
        new(p.Id, p.Title, p.Slug, CreateExcerpt(p.Content), p.CategoryId, p.Category?.Name ?? "Uncategorized", p.PublishedAt, TagParser.ToList(p.Tags));

    private static PostDetailDto ToDetail(Post p) =>
        new(p.Id, p.Title, p.Slug, CreateExcerpt(p.Content), (p.Content ?? string.Empty).Trim(), p.CategoryId, p.Category?.Name ?? "Uncategorized", p.PublishedAt, TagParser.ToList(p.Tags));

    private static string CreateExcerpt(string content)
    {
        var normalized = string.Join(' ', content.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= ExcerptLength ? normalized : normalized[..ExcerptLength].TrimEnd() + "...";
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? currentPostId = null)
    {
        var baseSlug = SlugGenerator.Generate(title);

        var existingSlugs = await db.Posts.AsNoTracking()
            .Where(p => !currentPostId.HasValue || p.Id != currentPostId.Value)
            .Select(p => p.Slug)
            .ToListAsync();

        return SlugGenerator.MakeUnique(baseSlug, existingSlugs);
    }
}
