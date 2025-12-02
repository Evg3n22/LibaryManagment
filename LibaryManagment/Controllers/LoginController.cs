using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
using LibaryManagment.Models;

namespace LibaryManagment.Controllers;

public class LoginController : Controller
{
    private readonly IConfiguration _config;

    public LoginController(IConfiguration config)
    {
        _config = config;
    }

    // GET
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Verify(LoginModel usr)
    {
        // 🔹 1. Перевірка статичного адміна
        if (usr.username == "admin" && usr.password == "123")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usr.username),
                new Claim(ClaimTypes.Role, "admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // 🔹 2. Перевірка Librarian
        using (var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            await con.OpenAsync();

            var cmd = new MySqlCommand("SELECT * FROM Librarians WHERE Name=@username AND Password=@password", con);
            cmd.Parameters.AddWithValue("@username", usr.username);
            cmd.Parameters.AddWithValue("@password", usr.password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usr.username),
                    new Claim(ClaimTypes.Role, "lib"),
                    new Claim(ClaimTypes.Role, "moderator")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
        }

        // 🔹 3. Перевірка Student
        using (var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            await con.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT * FROM Students WHERE LOWER(StudentName)=@username AND Password=@password", con);
            cmd.Parameters.AddWithValue("@username", usr.username.ToLower());
            cmd.Parameters.AddWithValue("@password", usr.password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, reader["StudentName"].ToString()),
                    new Claim(ClaimTypes.Role, "user") // або reader["Role"].ToString()
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
        }


        // Якщо нікого не знайдено
        ViewBag.message = "Login Failed";
        return View("Login");
    }
}