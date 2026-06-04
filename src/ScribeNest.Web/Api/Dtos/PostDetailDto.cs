namespace ScribeNest.Web.Api.Dtos;

public record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Excerpt,
    string Content,
    int CategoryId,
    string Category,
    DateTime PublishedAt,
    IReadOnlyList<string> Tags
);
