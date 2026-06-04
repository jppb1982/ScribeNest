using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScribeNest.Web.Models;

public class PostsIndexVm
{
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    public IReadOnlyList<PostListItemVm> Posts { get; set; } = Array.Empty<PostListItemVm>();
}

public class PostListItemVm
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public int ReadingMinutes { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}
