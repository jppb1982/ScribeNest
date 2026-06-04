using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScribeNest.Application.Interfaces;
using ScribeNest.Domain.Entities;
using ScribeNest.Web.Models;
using ScribeNest.Web.Helpers;

public class PostsController(IUnitOfWork uow) : Controller
{
    private readonly IUnitOfWork _uow = uow;

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? categoryId, int page = 1)
    {
        const int pageSize = 6;
        page = Math.Max(1, page);

        var categories = (await _uow.Categories.ListAsync()).OrderBy(c => c.Name).ToList();
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var posts = await _uow.Posts.ListAsync(p =>
            (string.IsNullOrWhiteSpace(q)
                || p.Title.Contains(q)
                || p.Content.Contains(q)
                || p.Tags.Contains(q))
            && (!categoryId.HasValue || p.CategoryId == categoryId.Value));

        var orderedPosts = posts.OrderByDescending(p => p.PublishedAt).ToList();
        var totalCount = orderedPosts.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        if (page > totalPages) page = totalPages;

        var vm = new PostsIndexVm
        {
            Q = q,
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = categoryId == c.Id
            }),
            Posts = orderedPosts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => ToListItem(p, categoryNames))
                .ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var posts = await _uow.Posts.ListAsync(p => p.Slug == slug);
        var post = posts.FirstOrDefault();

        if (post is null) return NotFound();

        var categories = await _uow.Categories.ListAsync(c => c.Id == post.CategoryId);
        ViewBag.CategoryName = categories.FirstOrDefault()?.Name ?? "Uncategorized";
        ViewBag.ReadingMinutes = EstimateReadingMinutes(post.Content);
        ViewBag.Tags = TagParser.ToList(post.Tags);

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var cats = await _uow.Categories.ListAsync();
        var vm = new PostCreateVm
        {
            Categories = cats.OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVm vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(vm);
            return View(vm);
        }

        var slug = await GenerateUniqueSlugAsync(vm.Title);

        var post = new Post
        {
            Title = vm.Title.Trim(),
            Slug = slug,
            Content = vm.Content.Trim(),
            Tags = TagParser.Normalize(vm.Tags),
            CategoryId = vm.CategoryId,
            PublishedAt = DateTime.UtcNow
        };

        await _uow.Posts.AddAsync(post);
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Artículo creado correctamente.";
        return RedirectToAction(nameof(Index));
    }
        
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _uow.Posts.GetByIdAsync(id);
        if (p is null) return NotFound();

        var cats = await _uow.Categories.ListAsync();
        var vm = new PostEditVm
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Content = p.Content,
            Tags = p.Tags,
            CategoryId = p.CategoryId,
            Categories = cats.OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(vm);
            return View(vm);
        }

        var p = await _uow.Posts.GetByIdAsync(vm.Id);
        if (p is null) return NotFound();

        p.Title = vm.Title.Trim();
        p.Slug = await GenerateUniqueSlugAsync(vm.Title, p.Id);
        p.Content = vm.Content.Trim();
        p.Tags = TagParser.Normalize(vm.Tags);
        p.CategoryId = vm.CategoryId;

        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Artículo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _uow.Posts.GetByIdAsync(id);
        if (p is null) return NotFound();
        return View(p);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post is null) return NotFound();

        _uow.Posts.Delete(post);
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Artículo eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(PostCreateVm vm)
    {
        var cats = await _uow.Categories.ListAsync();
        vm.Categories = cats.OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
    }

    private async Task LoadCategoriesAsync(PostEditVm vm)
    {
        var cats = await _uow.Categories.ListAsync();
        vm.Categories = cats.OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? currentPostId = null)
    {
        var baseSlug = SlugGenerator.Generate(title);

        var posts = await _uow.Posts.ListAsync();
        var existingSlugs = posts
            .Where(p => !currentPostId.HasValue || p.Id != currentPostId.Value)
            .Select(p => p.Slug);

        return SlugGenerator.MakeUnique(baseSlug, existingSlugs);
    }

    private static PostListItemVm ToListItem(Post post, IReadOnlyDictionary<int, string> categoryNames)
    {
        return new PostListItemVm
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Category = categoryNames.TryGetValue(post.CategoryId, out var category) ? category : "Uncategorized",
            PublishedAt = post.PublishedAt,
            Excerpt = BuildExcerpt(post.Content),
            ReadingMinutes = EstimateReadingMinutes(post.Content),
            Tags = TagParser.ToList(post.Tags)
        };
    }

    private static string BuildExcerpt(string content)
    {
        var text = (content ?? string.Empty).Replace(Environment.NewLine, " ").Trim();
        return text.Length <= 170 ? text : string.Concat(text.AsSpan(0, 167), "...");
    }

    private static int EstimateReadingMinutes(string content)
    {
        var words = (content ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(words / 200d));
    }
}
