using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibaryManagment.Models;

namespace LibaryManagment.Controllers;

public class LoginController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }

    public List<LoginModel> PutValue()
    {
        var users = new List<LoginModel>
        {
            new LoginModel{id=1,username="admin",password="123"},
            new LoginModel{id=2,username="user",password="lala123"},

        };

        return users;
    }

    [HttpPost]
    public IActionResult Verify(LoginModel usr)
    {
        var u = PutValue();

        var ue = u.Where(u => u.username.Equals(usr.username));
        var up = ue.Where(p => p.password.Equals(usr.password));

        if (up.Count() == 1)
        {
            TempData["message"] = "Login Success";
            return RedirectToAction("Index", "Dashboard");
        }
        else
        {
            ViewBag.message = "Login Failed";
            return View("Index");
        }
    }
}