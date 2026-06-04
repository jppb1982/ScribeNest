using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScribeNest.Web.Models;

public class PostEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 120 caracteres")]
    public string Title { get; set; } = "";

    [StringLength(120, ErrorMessage = "El slug no puede superar los 120 caracteres")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "El contenido es obligatorio")]
    [MinLength(80, ErrorMessage = "El contenido debe tener al menos 80 caracteres")]
    public string Content { get; set; } = "";

    [StringLength(180, ErrorMessage = "Los tags no pueden superar los 180 caracteres")]
    public string? Tags { get; set; }

    [Required(ErrorMessage = "Seleccioná una categoría")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una categoría")]
    public int CategoryId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
}
