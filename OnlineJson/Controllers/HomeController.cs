using Microsoft.AspNetCore.Mvc;

namespace OnlineJson.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}