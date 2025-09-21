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

    
    /*[HttpPost]
    public IActionResult Create(BookModel model)
    {
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
        var cmd = new MySqlCommand("INSERT INTO Books (BookName, Author) VALUES (@BookName, @Author)", con); 
        cmd.Parameters.AddWithValue("@BookName", model.BookName);
        cmd.Parameters.AddWithValue("@Author", model.Author);
        con.Open();
        cmd.ExecuteNonQuery();
        return RedirectToAction("Index");
    }*/
    
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
