using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScribeNest.Web.Models;

public class PostCreateVm
{
    [Required, StringLength(120)]
    public string Title { get; set; } = "";

    [Required, StringLength(120)]
    public string Slug { get; set; } = "";

    [Required]
    public string Content { get; set; } = "";

    [Required(ErrorMessage = "Seleccioná una categoría")]
    public int CategoryId { get; set; }

     public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
}
