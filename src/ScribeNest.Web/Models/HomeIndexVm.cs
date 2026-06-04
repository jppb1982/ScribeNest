namespace ScribeNest.Web.Models;

public class HomeIndexVm
{
    public int TotalPosts { get; set; }
    public int TotalCategories { get; set; }
    public IReadOnlyList<PostListItemVm> FeaturedPosts { get; set; } = Array.Empty<PostListItemVm>();
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
}
