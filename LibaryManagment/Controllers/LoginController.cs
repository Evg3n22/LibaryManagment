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
                    new Claim(ClaimTypes.Role, "lib")
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

// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using LibaryManagment.Models;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using System.Security.Claims;
// namespace LibaryManagment.Controllers;
//
// public class LoginController : Controller
// {
//     // GET
//     public IActionResult Login()
//     {
//         return View();
//     }
//
//     public List<LoginModel> PutValue()
//     {
//         var users = new List<LoginModel>
//         {
//             new LoginModel{id=1,username="admin",password="123", role="admin"},
//             // new LoginModel{id=2,username="user",password="lala123", role="user"},
//             // new LoginModel{id=3,username="librarian",password="snake", role="lib"}
//
//         };
//
//         return users;
//     }
//
//     [HttpPost]
//     public async Task<IActionResult> Verify(LoginModel usr)
//     {
//         var users = PutValue();
//         var user = users.FirstOrDefault(u => u.username == usr.username && u.password == usr.password);
//
//         if (user != null)
//         {
//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.Name, user.username),
//                 new Claim(ClaimTypes.Role, user.role) // <-- додаємо роль
//             };
//
//             var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//             var authProperties = new AuthenticationProperties
//             {
//                 IsPersistent = true // залишатися залогіненим після перезавантаження
//             };
//
//             await HttpContext.SignInAsync(
//                 CookieAuthenticationDefaults.AuthenticationScheme,
//                 new ClaimsPrincipal(claimsIdentity),
//                 authProperties);
//
//             return RedirectToAction("Index", "Home");
//         }
//         else
//         {
//             ViewBag.message = "Login Failed";
//             return View("Login");
//         }
//     }
// }



//
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using LibaryManagment.Models;
// using LibaryManagment.Data;
//
// namespace LibaryManagment.Controllers
// {
//     public class LoginController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//
//         public LoginController(ApplicationDbContext context)
//         {
//             _context = context;
//         }
//
//         public IActionResult Login()
//         {
//             return View();
//         }
//
//         [HttpPost]
//         public async Task<IActionResult> Verify(LoginModel usr)
//         {
//             // 1. Check static admin
//             if (usr.username == "admin" && usr.password == "123")
//             {
//                 var claims = new List<Claim>
//                 {
//                     new Claim(ClaimTypes.Name, usr.username),
//                     new Claim(ClaimTypes.Role, "admin")
//                 };
//
//                 var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//
//                 await HttpContext.SignInAsync(
//                     CookieAuthenticationDefaults.AuthenticationScheme,
//                     new ClaimsPrincipal(claimsIdentity));
//
//                 return RedirectToAction("Index", "Home");
//             }
//
//             // 2. Check Librarian
//             var librarian = await _context.Librarians
//                 .FirstOrDefaultAsync(l => l.Name == usr.username && l.Password == usr.password);
//
//             if (librarian != null)
//             {
//                 var claims = new List<Claim>
//                 {
//                     new Claim(ClaimTypes.Name, usr.username),
//                     new Claim(ClaimTypes.Role, "lib")
//                 };
//
//                 var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//
//                 await HttpContext.SignInAsync(
//                     CookieAuthenticationDefaults.AuthenticationScheme,
//                     new ClaimsPrincipal(claimsIdentity));
//
//                 return RedirectToAction("Index", "Home");
//             }
//
//             // 3. Check Student
//             var student = await _context.Students
//                 .FirstOrDefaultAsync(s => s.StudentName.ToLower() == usr.username.ToLower() 
//                                        && s.Password == usr.password);
//
//             if (student != null)
//             {
//                 var claims = new List<Claim>
//                 {
//                     new Claim(ClaimTypes.Name, student.StudentName),
//                     new Claim(ClaimTypes.Role, "user")
//                 };
//
//                 var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//
//                 await HttpContext.SignInAsync(
//                     CookieAuthenticationDefaults.AuthenticationScheme,
//                     new ClaimsPrincipal(claimsIdentity));
//
//                 return RedirectToAction("Index", "Home");
//             }
//
//             // Login failed
//             ViewBag.message = "Login Failed";
//             return View("Login");
//         }
//
//         public async Task<IActionResult> Logout()
//         {
//             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//             return RedirectToAction("Login");
//         }
//     }
// }