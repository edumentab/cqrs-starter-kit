using System.Diagnostics;
using AspFrontend.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using AspFrontend.Models;

namespace AspFrontend.Controllers;

[IncludeLayoutData]
public class HomeController : Controller
{
    readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}