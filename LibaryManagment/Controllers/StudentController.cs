using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _config;
        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var students = new List<StudentModel>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Students", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new StudentModel
                {
                    StudentId = (int)reader["StudentId"],
                    StudentName = reader["StudentName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString()
                });
            }
            return View(students);
        }
        
        [Authorize(Roles = "admin,lib")]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Create(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("INSERT INTO Students (StudentName, Email, Phone) VALUES (@Name, @Email, @Phone)", con);
            cmd.Parameters.AddWithValue("@Name", model.StudentName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "admin,lib")]
        public IActionResult Edit(int id)
        {
            StudentModel student = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Students WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                student.StudentId = (int)reader["StudentId"];
                student.StudentName = reader["StudentName"].ToString();
                student.Email = reader["Email"].ToString();
                student.Phone = reader["Phone"].ToString();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("UPDATE Students SET StudentName=@Name, Email=@Email, Phone=@Phone WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@Name", model.StudentName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            cmd.Parameters.AddWithValue("@id", model.StudentId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "admin,lib")]
        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("DELETE FROM Students WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}