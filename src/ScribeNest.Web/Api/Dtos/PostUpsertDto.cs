using System.ComponentModel.DataAnnotations;

namespace ScribeNest.Web.Api.Dtos;

public class PostUpsertDto
{
    [Required, StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [StringLength(140)]
    public string? Slug { get; set; }

    [Required, MinLength(80)]
    public string Content { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Tags { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
