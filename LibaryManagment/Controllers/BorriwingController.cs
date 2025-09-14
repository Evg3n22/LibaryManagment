using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;

namespace LibraryManagement.Controllers
{
    public class BorrowingController : Controller
    {
        private readonly IConfiguration _config;

        public BorrowingController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            List<BorrowingModel> list = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand("SELECT * FROM Borrowings", con); 
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BorrowingModel
                {
                    BorrowingId = Convert.ToInt32(reader["BorrowingId"]),
                    StudentName = reader["StudentName"].ToString(),
                    BookName = reader["BookName"].ToString(),
                    BorrowDate = Convert.ToDateTime(reader["BorrowDate"]),
                    ReturnedDate = Convert.ToDateTime(reader["ReturnedDate"])
                });
            }
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            List<string> students = new();
            List<string> books = new();

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Отримати студентів
            var studentCmd = new MySqlCommand("SELECT StudentName FROM Students", con);
            using (var reader = studentCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    students.Add(reader["StudentName"].ToString());
                }
            }

            // Отримати книги
            var bookCmd = new MySqlCommand("SELECT BookName FROM Books", con);
            using (var reader = bookCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    books.Add(reader["BookName"].ToString());
                }
            }

            ViewBag.Students = students;
            ViewBag.Books = books;

            return View();
        }


        [HttpPost]
        public IActionResult Create(BorrowingModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand(
                "INSERT INTO Borrowings (StudentName, BookName, BorrowDate, ReturnedDate) VALUES (@studentname, @bookname, @borrowdate, @returneddate)", 
                con); 
            cmd.Parameters.AddWithValue("@studentname", model.StudentName);
            cmd.Parameters.AddWithValue("@bookname", model.BookName);
            cmd.Parameters.AddWithValue("@borrowdate", model.BorrowDate);
            cmd.Parameters.AddWithValue("@returneddate", model.ReturnedDate);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult Edit(int id)
        {
            BorrowingModel model = new();

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Завантажуємо Borrowing
            var cmd = new MySqlCommand("SELECT * FROM Borrowings WHERE BorrowingId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    model.BorrowingId = Convert.ToInt32(reader["BorrowingId"]);
                    model.StudentName = reader["StudentName"].ToString();
                    model.BookName = reader["BookName"].ToString();
                    model.BorrowDate = Convert.ToDateTime(reader["BorrowDate"]);
                    model.ReturnedDate = Convert.ToDateTime(reader["ReturnedDate"]);
                }
            }

            // Завантажуємо студентів
            List<string> students = new();
            var studentCmd = new MySqlCommand("SELECT StudentName FROM Students", con);
            using (var studentReader = studentCmd.ExecuteReader())
            {
                while (studentReader.Read())
                {
                    students.Add(studentReader["StudentName"].ToString());
                }
            }

            // Завантажуємо книги
            List<string> books = new();
            var bookCmd = new MySqlCommand("SELECT BookName FROM Books", con);
            using (var bookReader = bookCmd.ExecuteReader())
            {
                while (bookReader.Read())
                {
                    books.Add(bookReader["BookName"].ToString());
                }
            }

            ViewBag.Students = students;
            ViewBag.Books = books;

            return View(model);
        }


        [HttpPost]
        public IActionResult Edit(BorrowingModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            var cmd = new MySqlCommand(
                "UPDATE Borrowings SET StudentName=@studentname, BookName=@bookname, BorrowDate=@borrowdate, ReturnedDate=@returneddate WHERE BorrowingId=@id", 
                con); // ✅ зміна
            cmd.Parameters.AddWithValue("@studentname", model.StudentName);
            cmd.Parameters.AddWithValue("@bookname", model.BookName);
            cmd.Parameters.AddWithValue("@borrowdate", model.BorrowDate);
            cmd.Parameters.AddWithValue("@returneddate", model.ReturnedDate);
            cmd.Parameters.AddWithValue("@id", model.BorrowingId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); // ✅ зміна
            var cmd = new MySqlCommand("DELETE FROM Borrowings WHERE BorrowingId=@id", con); // ✅ зміна
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}