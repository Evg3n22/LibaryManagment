/*
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;
namespace LibaryManagment.Controllers;
using Microsoft.AspNetCore.Authorization;

public class BookController : Controller
{
    private readonly IConfiguration _config;

    public BookController(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult Index()
    {
        var books = new List<BookModel>();
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand("SELECT * FROM Books", con); 
        con.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new BookModel
            {
                BookId = Convert.ToInt32(reader["BookId"]), 
                BookName = reader["BookName"].ToString(),
                Author = reader["Author"].ToString(),
                Available = Convert.ToBoolean(reader["Available"])
            });
        }
        return View(books);
    }
    
    [Authorize(Roles = "admin,lib")]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Create(BookModel model)
    {
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand(
            "INSERT INTO Books (BookName, Author, Available) VALUES (@BookName, @Author, 1)", con); 
        cmd.Parameters.AddWithValue("@BookName", model.BookName);
        cmd.Parameters.AddWithValue("@Author", model.Author);
        con.Open();
        cmd.ExecuteNonQuery();
        return RedirectToAction("Index");
    }
    
    
    [Authorize(Roles = "admin,lib")]
    public IActionResult Edit(int id)
    {
        BookModel book = new();
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand("SELECT * FROM Books WHERE BookId=@id", con); 
        cmd.Parameters.AddWithValue("@id", id);
        con.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            book.BookId = Convert.ToInt32(reader["BookId"]); 
            book.BookName = reader["BookName"].ToString();
            book.Author = reader["Author"].ToString();
        }
        return View(book);
    }
    
    [HttpPost]
    public IActionResult Edit(BookModel model)
    {
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand("UPDATE Books SET BookName=@BookName, Author=@Author WHERE BookId=@id", con); 
        cmd.Parameters.AddWithValue("@BookName", model.BookName);
        cmd.Parameters.AddWithValue("@Author", model.Author);
        cmd.Parameters.AddWithValue("@id", model.BookId);
        con.Open();
        cmd.ExecuteNonQuery();
        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "admin,lib")]
    public IActionResult Delete(int id)
    {
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand("DELETE FROM Books WHERE BookId=@id", con); 
        cmd.Parameters.AddWithValue("@id", id);
        con.Open();
        cmd.ExecuteNonQuery();
        return RedirectToAction("Index");
    }
}
*/

using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibaryManagment.Controllers
{
    public class BookController : Controller
    {
        private readonly IConfiguration _config;

        public BookController(IConfiguration config)
        {
            _config = config;
        }

        // GET: List of books
        public IActionResult Index()
        {
            var books = new List<BookModel>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Books", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                books.Add(new BookModel
                {
                    BookId = Convert.ToInt32(reader["BookId"]),
                    BookName = reader["BookName"].ToString(),
                    AuthorName = reader["AuthorName"].ToString(),
                    AuthorID = Convert.ToInt32(reader["AuthorID"]),
                    CopiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]),
                    CopiesTotal = Convert.ToInt32(reader["CopiesTotal"]),
                    Year = Convert.ToInt32(reader["Year"]),
                    Publisher = reader["Publisher"].ToString(),
                    BookDescription = reader["BookDescription"].ToString(),
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    BookImage = reader["BookImage"] as byte[]
                });
            }
            return View(books);
        }

        // GET: Create book
        [Authorize(Roles = "admin,lib")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create book
        [HttpPost]
        public IActionResult Create(BookModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand(
                @"INSERT INTO Books 
                (BookName, AuthorName, AuthorID, CopiesAvailable, CopiesTotal, Year, Publisher, BookDescription, CategoryID, BookImage) 
                VALUES 
                (@BookName, @AuthorName, @AuthorID, @CopiesAvailable, @CopiesTotal, @Year, @Publisher, @BookDescription, @CategoryID, @BookImage)", con);

            cmd.Parameters.AddWithValue("@BookName", model.BookName);
            cmd.Parameters.AddWithValue("@AuthorName", model.AuthorName);
            cmd.Parameters.AddWithValue("@AuthorID", model.AuthorID);
            cmd.Parameters.AddWithValue("@CopiesAvailable", model.CopiesAvailable);
            cmd.Parameters.AddWithValue("@CopiesTotal", model.CopiesTotal);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@Publisher", model.Publisher);
            cmd.Parameters.AddWithValue("@BookDescription", model.BookDescription);
            cmd.Parameters.AddWithValue("@CategoryID", model.CategoryID);
            cmd.Parameters.AddWithValue("@BookImage", model.BookImage ?? (object)DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        // GET: Edit book
        [Authorize(Roles = "admin,lib")]
        public IActionResult Edit(int id)
        {
            BookModel book = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Books WHERE BookId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                book.BookId = Convert.ToInt32(reader["BookId"]);
                book.BookName = reader["BookName"].ToString();
                book.AuthorName = reader["AuthorName"].ToString();
                book.AuthorID = Convert.ToInt32(reader["AuthorID"]);
                book.CopiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]);
                book.CopiesTotal = Convert.ToInt32(reader["CopiesTotal"]);
                book.Year = Convert.ToInt32(reader["Year"]);
                book.Publisher = reader["Publisher"].ToString();
                book.BookDescription = reader["BookDescription"].ToString();
                book.CategoryID = Convert.ToInt32(reader["CategoryID"]);
                book.BookImage = reader["BookImage"] as byte[];
            }
            return View(book);
        }

        // POST: Edit book
        [HttpPost]
        public IActionResult Edit(BookModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand(
                @"UPDATE Books SET 
                BookName=@BookName, AuthorName=@AuthorName, AuthorID=@AuthorID, 
                CopiesAvailable=@CopiesAvailable, CopiesTotal=@CopiesTotal, 
                Year=@Year, Publisher=@Publisher, BookDescription=@BookDescription, 
                CategoryID=@CategoryID, BookImage=@BookImage
                WHERE BookId=@id", con);

            cmd.Parameters.AddWithValue("@BookName", model.BookName);
            cmd.Parameters.AddWithValue("@AuthorName", model.AuthorName);
            cmd.Parameters.AddWithValue("@AuthorID", model.AuthorID);
            cmd.Parameters.AddWithValue("@CopiesAvailable", model.CopiesAvailable);
            cmd.Parameters.AddWithValue("@CopiesTotal", model.CopiesTotal);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@Publisher", model.Publisher);
            cmd.Parameters.AddWithValue("@BookDescription", model.BookDescription);
            cmd.Parameters.AddWithValue("@CategoryID", model.CategoryID);
            cmd.Parameters.AddWithValue("@BookImage", model.BookImage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", model.BookId);

            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        // GET: Delete book
        [Authorize(Roles = "admin,lib")]
        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("DELETE FROM Books WHERE BookId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
