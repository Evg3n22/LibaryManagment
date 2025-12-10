using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization; // Required for [AllowAnonymous]

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
    
    public IActionResult Debug()
    {
        // 1. Get the assembly where the app is running
        var assembly = typeof(Program).Assembly;
    
        // 2. Ask: "What are the exact names of the resources you have embedded?"
        var resourceNames = assembly.GetManifestResourceNames();

        // 3. Ask: "What is the Full Name of the SharedResource class?"
        var className = typeof(SharedResource).FullName;

        return Ok(new 
        { 
            MyClass = className, 
            FoundResources = resourceNames 
        });
    }
    
    public IActionResult TestLoc([FromServices] IHtmlLocalizer<SharedResource> localizer)
    {
        return Ok(new {
            WelcomeEN = localizer["WelcomeMessage"].Value,
            Culture = System.Globalization.CultureInfo.CurrentUICulture.Name
        });
    }
    
    [AllowAnonymous]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl);
    }
}