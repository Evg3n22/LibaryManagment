using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization; // Required for [AllowAnonymous]

namespace LibaryManagment.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // GET: Home Page
    [AllowAnonymous] // <--- Guests must be able to see the landing page
    public IActionResult Index()
    {
        return View();
    }

    // GET: Privacy Page
    [AllowAnonymous] // <--- Guests should be able to read privacy policy
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