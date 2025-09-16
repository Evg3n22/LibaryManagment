using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;


namespace LibaryManagment.Controllers;

public class LibrarianController : Controller
{
    private readonly IConfiguration _config;

        public LibrarianController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var librarians = new List<LibrarianModel>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand("SELECT * FROM Librarians", con); 
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                librarians.Add(new LibrarianModel
                {
                    LibrarianId = Convert.ToInt32(reader["LibrarianId"]), 
                    Name = reader["Name"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]), 
                    Phone = reader["Phone"].ToString()
                });
            }
            return View(librarians);
        }
        
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand("INSERT INTO Librarians (Name, Age, Phone) VALUES (@Name, @Age, @Phone)", con);
            cmd.Parameters.AddWithValue("@Name", model.Name);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "admin")]
        public IActionResult Edit(int id)
        {
            LibrarianModel librarian = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand("SELECT * FROM Librarians WHERE LibrarianId=@id", con); 
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                librarian.LibrarianId = Convert.ToInt32(reader["LibrarianId"]); 
                librarian.Name = reader["Name"].ToString();
                librarian.Age = Convert.ToInt32(reader["Age"]); 
                librarian.Phone = reader["Phone"].ToString();
            }
            return View(librarian);
        }

        [HttpPost]
        public IActionResult Edit(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("UPDATE Librarians SET Name=@Name, Age=@Age, Phone=@Phone WHERE LibrarianId=@id", con); // ✅ зміна
            cmd.Parameters.AddWithValue("@Name", model.Name);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            cmd.Parameters.AddWithValue("@id", model.LibrarianId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand("DELETE FROM Librarians WHERE LibrarianId=@id", con); 
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
}