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
        
    // GET: Book Details
    public IActionResult Details(int id)
    {
        BookModel book = new();
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    
        var query = @"SELECT b.BookId, b.BookName, b.AuthorName, b.AuthorID, 
                     b.CopiesAvailable, b.CopiesTotal, b.Year, b.Publisher, 
                     b.BookDescription, b.CategoryID, b.BookImage,
                     c.CategoryName
              FROM Books b 
              LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
              WHERE b.BookId = @id";
    
        var cmd = new MySqlCommand(query, con);
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
            book.CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : "Uncategorized";
            book.BookImage = reader["BookImage"] as byte[];
        }
    
        return View(book);
    }
    
    // GET: List of books with search and filters
    public IActionResult Index(string searchTerm, int? categoryId, string availability, int? year)
    {
        var books = new List<BookModel>();
        using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
        
        // Updated query to JOIN with Categories table to get CategoryName
        var query = @"SELECT b.BookId, b.BookName, b.AuthorName, b.AuthorID, 
                             b.CopiesAvailable, b.CopiesTotal, b.Year, b.Publisher, 
                             b.BookDescription, b.CategoryID, b.BookImage,
                             c.CategoryName
                      FROM Books b 
                      LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
                      WHERE 1=1";
        var parameters = new List<MySqlParameter>();
        
        // Smart search with prefixes
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            
            if (searchTerm.StartsWith("author:", StringComparison.OrdinalIgnoreCase))
            {
                // Search by author only
                var authorSearch = searchTerm.Substring(7).Trim();
                query += " AND b.AuthorName LIKE @searchTerm";
                parameters.Add(new MySqlParameter("@searchTerm", $"%{authorSearch}%"));
            }
            else if (searchTerm.StartsWith("publisher:", StringComparison.OrdinalIgnoreCase))
            {
                // Search by publisher only
                var publisherSearch = searchTerm.Substring(10).Trim();
                query += " AND b.Publisher LIKE @searchTerm";
                parameters.Add(new MySqlParameter("@searchTerm", $"%{publisherSearch}%"));
            }
            else
            {
                // Default: search by book name only
                query += " AND b.BookName LIKE @searchTerm";
                parameters.Add(new MySqlParameter("@searchTerm", $"%{searchTerm}%"));
            }
        }
        
        // Filter by category
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query += " AND b.CategoryID = @categoryId";
            parameters.Add(new MySqlParameter("@categoryId", categoryId.Value));
        }
        
        // Filter by availability
        if (!string.IsNullOrEmpty(availability))
        {
            if (availability == "available")
            {
                query += " AND b.CopiesAvailable > 0";
            }
            else if (availability == "unavailable")
            {
                query += " AND b.CopiesAvailable = 0";
            }
        }
        
        // Filter by year
        if (year.HasValue && year.Value > 0)
        {
            query += " AND b.Year = @year";
            parameters.Add(new MySqlParameter("@year", year.Value));
        }
        
        var cmd = new MySqlCommand(query, con);
        foreach (var param in parameters)
        {
            cmd.Parameters.Add(param);
        }
        
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
                CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : "Uncategorized",
                BookImage = reader["BookImage"] as byte[]
            });
        }
        
        // Get categories for filter dropdown from Categories table
        var categories = new List<dynamic>();
        using (var con2 = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            var cmdCat = new MySqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", con2);
            con2.Open();
            using var readerCat = cmdCat.ExecuteReader();
            while (readerCat.Read())
            {
                categories.Add(new
                {
                    CategoryID = Convert.ToInt32(readerCat["CategoryID"]),
                    CategoryName = readerCat["CategoryName"].ToString()
                });
            }
        }
        
        // Get unique years for filter dropdown
        var years = new List<int>();
        using (var con3 = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            var cmdYear = new MySqlCommand("SELECT DISTINCT Year FROM Books WHERE Year IS NOT NULL ORDER BY Year DESC", con3);
            con3.Open();
            using var readerYear = cmdYear.ExecuteReader();
            while (readerYear.Read())
            {
                years.Add(Convert.ToInt32(readerYear["Year"]));
            }
        }
        
        // Pass filter values back to view
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryId = categoryId?.ToString();
        ViewBag.Availability = availability;
        ViewBag.Year = year?.ToString();
        ViewBag.Categories = categories;
        ViewBag.Years = years;
        
        return View(books);
    }
        
        
        
        // GET: Create book
        [Authorize(Roles = "admin,lib")]
        public IActionResult Create()
        {
            var authors = new List<AuthorModel>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT AuthorID, AuthorName FROM Author", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                authors.Add(new AuthorModel
                {
                    AuthorID = Convert.ToInt32(reader["AuthorID"]),
                    AuthorName = reader["AuthorName"].ToString()
                });
            }
            ViewBag.Authors = authors;
            return View();
        }
        
        // POST: Create book
        [HttpPost]
        public async Task<IActionResult> Create(BookModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var getAuthorNameCmd = new MySqlCommand("SELECT AuthorName FROM Author WHERE AuthorID = @AuthorID", con);
            getAuthorNameCmd.Parameters.AddWithValue("@AuthorID", model.AuthorID);
            con.Open();
            var authorName = getAuthorNameCmd.ExecuteScalar()?.ToString();
            model.AuthorName = authorName;

            // Handle the uploaded image file
            if (model.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                model.BookImage = memoryStream.ToArray();
            }

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

            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        
        // GET: Edit book
        [Authorize(Roles = "admin,lib")]
        public IActionResult Edit(int id)
        {
            BookModel book = new();
            // First database connection for fetching the book details
            using (var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
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
            }
            
            // Second, separate database connection for fetching the authors list
            var authors = new List<AuthorModel>();
            using (var con1 = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var cmd1 = new MySqlCommand("SELECT AuthorID, AuthorName FROM Author", con1);
                con1.Open();
                using var reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    authors.Add(new AuthorModel
                    {
                        AuthorID = Convert.ToInt32(reader1["AuthorID"]),
                        AuthorName = reader1["AuthorName"].ToString()
                    });
                }
            }
            ViewBag.Authors = authors;
            return View(book);
        }
        
        // POST: Edit book
        [HttpPost]
        public async Task<IActionResult> Edit(BookModel model)
        {
            // Handle the uploaded image file
            if (model.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                model.BookImage = memoryStream.ToArray();
            }

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand(
                @"UPDATE Books SET 
                BookName=@BookName, AuthorName=@AuthorName, AuthorID=@AuthorID, 
                CopiesAvailable=@CopiesAvailable, CopiesTotal=@CopiesTotal, 
                Year=@Year, Publisher=@Publisher, BookDescription=@BookDescription, 
                CategoryID=@CategoryID, BookImage=@BookImage
                WHERE BookId=@id", con);

            // You also need to fetch the AuthorName here for the edit to work correctly
            var getAuthorNameCmd = new MySqlCommand("SELECT AuthorName FROM Author WHERE AuthorID = @AuthorID", con);
            getAuthorNameCmd.Parameters.AddWithValue("@AuthorID", model.AuthorID);
            con.Open();
            var authorName = getAuthorNameCmd.ExecuteScalar()?.ToString();
            model.AuthorName = authorName;
            
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
