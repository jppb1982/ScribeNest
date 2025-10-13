namespace ScribeNest.Web.Api.Dtos;

public record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string Category,
    DateTime PublishedAt
);
