namespace ScribeNest.Web.Api.Dtos;

public record AiSuggestionRequest(
    string Title,
    string Content,
    string? Category,
    string? Tags);
