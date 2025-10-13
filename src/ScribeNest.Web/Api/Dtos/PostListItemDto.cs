namespace ScribeNest.Web.Api.Dtos;

public record PostListItemDto(
    int Id,
    string Title,
    string Slug,
    string Category,
    DateTime PublishedAt
);
