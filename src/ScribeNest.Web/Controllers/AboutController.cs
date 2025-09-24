using Microsoft.AspNetCore.Mvc;

namespace ScribeNest.Web.Controllers;

public class AboutController : Controller
{
    public IActionResult Index() => View();
}
