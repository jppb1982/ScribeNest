using Microsoft.AspNetCore.Mvc;

namespace ScribeNest.Web.Controllers;

public class ErrorsController : Controller
{
    [Route("Errors/Http/{code:int}")]
    public IActionResult Http(int code)
    {
        return code switch
        {
            404 => View("NotFound"),
            _ => View("Generic", code)
        };
    }
}
