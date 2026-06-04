namespace ScribeNest.Web.Api.Dtos;

public record PostListItemDto(
    int Id,
    string Title,
    string Slug,
    string Excerpt,
    int CategoryId,
    string Category,
    DateTime PublishedAt,
    IReadOnlyList<string> Tags
);
