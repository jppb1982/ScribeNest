using Microsoft.AspNetCore.Mvc;
using ScribeNest.Application.Interfaces;
using ScribeNest.Domain.Entities;
using ScribeNest.Web.Helpers;
using ScribeNest.Web.Models;

public class DashboardController(IUnitOfWork uow) : Controller
{
    private readonly IUnitOfWork _uow = uow;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = (await _uow.Posts.ListAsync()).OrderByDescending(p => p.PublishedAt).ToList();
        var categories = (await _uow.Categories.ListAsync()).OrderBy(c => c.Name).ToList();
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var vm = new DashboardVm
        {
            TotalPosts = posts.Count,
            TotalCategories = categories.Count,
            LatestPosts = posts.Take(5).Select(p => ToListItem(p, categoryNames)).ToList(),
            PostsByCategory = categories
                .Select(c => new CategoryMetricVm
                {
                    Category = c.Name,
                    Count = posts.Count(p => p.CategoryId == c.Id)
                })
                .ToList(),
            TopTags = posts
                .SelectMany(p => TagParser.ToList(p.Tags))
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .Select(g => new TagMetricVm { Tag = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .ThenBy(t => t.Tag)
                .ToList()
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
            ReadingMinutes = EstimateReadingMinutes(post.Content),
            Tags = TagParser.ToList(post.Tags)
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
