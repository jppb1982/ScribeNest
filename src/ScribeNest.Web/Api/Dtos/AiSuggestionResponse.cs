namespace ScribeNest.Web.Api.Dtos;

public record AiSuggestionResponse(
    string SuggestedTitle,
    string Summary,
    string Excerpt,
    IReadOnlyList<string> SuggestedTags,
    string ExplainForJuniors,
    IReadOnlyList<string> Warnings,
    string EditorialConsistency,
    IReadOnlyList<string> EditorialNotes);
