using System.ComponentModel.DataAnnotations;

namespace ScribeNest.Web.Api.Dtos;

public class CategoryUpsertDto
{
    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;
}
