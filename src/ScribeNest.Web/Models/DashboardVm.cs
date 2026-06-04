namespace ScribeNest.Web.Models;

public class DashboardVm
{
    public int TotalPosts { get; set; }
    public int TotalCategories { get; set; }
    public IReadOnlyList<PostListItemVm> LatestPosts { get; set; } = Array.Empty<PostListItemVm>();
    public IReadOnlyList<CategoryMetricVm> PostsByCategory { get; set; } = Array.Empty<CategoryMetricVm>();
    public IReadOnlyList<TagMetricVm> TopTags { get; set; } = Array.Empty<TagMetricVm>();
}

public class CategoryMetricVm
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}


public class TagMetricVm
{
    public string Tag { get; set; } = string.Empty;
    public int Count { get; set; }
}
