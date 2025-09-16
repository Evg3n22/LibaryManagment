using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
namespace LibaryManagment.Controllers;

public class LoginController : Controller
{
    // GET
    public IActionResult Login()
    {
        return View();
    }

    public List<LoginModel> PutValue()
    {
        var users = new List<LoginModel>
        {
            new LoginModel{id=1,username="admin",password="123", role="admin"},
            new LoginModel{id=2,username="user",password="lala123", role="user"},
            new LoginModel{id=3,username="librarian",password="snake", role="lib"}

        };

        return users;
    }

    [HttpPost]
    public async Task<IActionResult> Verify(LoginModel usr)
    {
        var users = PutValue();
        var user = users.FirstOrDefault(u => u.username == usr.username && u.password == usr.password);

        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Role, user.role) // <-- додаємо роль
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // залишатися залогіненим після перезавантаження
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }
        else
        {
            ViewBag.message = "Login Failed";
            return View("Login");
        }
    }
}