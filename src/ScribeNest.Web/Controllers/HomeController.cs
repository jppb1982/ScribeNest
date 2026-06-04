using Microsoft.AspNetCore.Mvc;
using ScribeNest.Application.Interfaces;
using ScribeNest.Domain.Entities;
using ScribeNest.Web.Models;

public class HomeController(IUnitOfWork uow) : Controller
{
    private readonly IUnitOfWork _uow = uow;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = (await _uow.Posts.ListAsync()).OrderByDescending(p => p.PublishedAt).ToList();
        var categories = (await _uow.Categories.ListAsync()).OrderBy(c => c.Name).ToList();
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var vm = new HomeIndexVm
        {
            TotalPosts = posts.Count,
            TotalCategories = categories.Count,
            Categories = categories.Select(c => c.Name).ToList(),
            FeaturedPosts = posts.Take(3).Select(p => ToListItem(p, categoryNames)).ToList()
        };

        return View(vm);
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
            ReadingMinutes = EstimateReadingMinutes(post.Content)
        };
    }

    private static string BuildExcerpt(string content)
    {
        var text = (content ?? string.Empty).Replace(Environment.NewLine, " ").Trim();
        return text.Length <= 160 ? text : string.Concat(text.AsSpan(0, 157), "...");
    }

    private static int EstimateReadingMinutes(string content)
    {
        var words = (content ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(words / 200d));
    }
}
