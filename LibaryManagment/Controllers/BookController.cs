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
    // public IActionResult Details(int id)
    // {
    //     BookModel book = new();
    //     using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    //
    //     var query = @"SELECT b.BookId, b.BookName, b.AuthorName, b.AuthorID, 
    //                  b.CopiesAvailable, b.CopiesTotal, b.Year, b.Publisher, 
    //                  b.BookDescription, b.CategoryID, b.BookImage,
    //                  c.CategoryName
    //           FROM Books b 
    //           LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
    //           WHERE b.BookId = @id";
    //
    //     var cmd = new MySqlCommand(query, con);
    //     cmd.Parameters.AddWithValue("@id", id);
    //     con.Open();
    //
    //     using var reader = cmd.ExecuteReader();
    //     if (reader.Read())
    //     {
    //         book.BookId = Convert.ToInt32(reader["BookId"]);
    //         book.BookName = reader["BookName"].ToString();
    //         book.AuthorName = reader["AuthorName"].ToString();
    //         book.AuthorID = Convert.ToInt32(reader["AuthorID"]);
    //         book.CopiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]);
    //         book.CopiesTotal = Convert.ToInt32(reader["CopiesTotal"]);
    //         book.Year = Convert.ToInt32(reader["Year"]);
    //         book.Publisher = reader["Publisher"].ToString();
    //         book.BookDescription = reader["BookDescription"].ToString();
    //         book.CategoryID = Convert.ToInt32(reader["CategoryID"]);
    //         book.CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : "Uncategorized";
    //         book.BookImage = reader["BookImage"] as byte[];
    //     }
    //     
    //     
    //
    //     return View(book);
    // }
    //
    
    
    public IActionResult Details(int id)
{
    BookModel book = new();
    // Use a list to store reviews for this book
    List<ReviewModel> reviews = new List<ReviewModel>();

    using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    con.Open();

    // Fetch Book Details
    var query = @"SELECT b.*, c.CategoryName 
                  FROM Books b 
                  LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
                  WHERE b.BookId = @id";

    using (var cmd = new MySqlCommand(query, con))
    {
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            book.BookId = Convert.ToInt32(reader["BookId"]);
            book.BookName = reader["BookName"].ToString();
            book.AuthorName = reader["AuthorName"].ToString();
            book.BookDescription = reader["BookDescription"].ToString();
            book.Year = Convert.ToInt32(reader["Year"]);
            book.Publisher = reader["Publisher"].ToString();
            book.CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : "Uncategorized";
            book.CopiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]);
            book.BookImage = reader["BookImage"] as byte[];
        }
    }

    // Fetch Reviews for this book
    var reviewQuery = "SELECT * FROM Reviews WHERE BookId = @id ORDER BY CreatedAt DESC";
    using (var cmdReview = new MySqlCommand(reviewQuery, con))
    {
        cmdReview.Parameters.AddWithValue("@id", id);
        using var reader = cmdReview.ExecuteReader();
        while (reader.Read())
        {
            reviews.Add(new ReviewModel
            {
                ReviewId = Convert.ToInt32(reader["ReviewId"]),
                UserName = reader["UserName"].ToString(),
                Content = reader["Content"].ToString(),
                Rating = Convert.ToInt32(reader["Rating"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            });
        }
    }

    // Pass reviews to the view via ViewBag
    ViewBag.Reviews = reviews;

    return View(book);
}

    
    
    // GET: List of books with search and filters
    public async Task<IActionResult> Index(string searchTerm, int? categoryId, string availability, int? year)
{
    var books = new List<BookModel>();
    var categories = new List<dynamic>();
    var years = new List<int>();

    // 1. Use "Async" Task to free up server threads
    // 2. Open the connection ONCE for all operations
    using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    await con.OpenAsync();

    // --- PART A: FETCH BOOKS ---
    var query = @"SELECT b.BookId, b.BookName, b.AuthorName, b.AuthorID, 
                         b.CopiesAvailable, b.CopiesTotal, b.Year, b.Publisher, 
                         b.BookDescription, b.CategoryID, b.BookImage,
                         c.CategoryName
                  FROM Books b 
                  LEFT JOIN Categories c ON b.CategoryID = c.CategoryID 
                  WHERE 1=1";

    var parameters = new List<MySqlParameter>();

    // Logic for search prefixes refactored for cleaner reading
    if (!string.IsNullOrEmpty(searchTerm))
    {
        searchTerm = searchTerm.Trim();
        string columnToSearch = "b.BookName"; // Default
        string cleanTerm = searchTerm;

        if (searchTerm.StartsWith("author:", StringComparison.OrdinalIgnoreCase))
        {
            columnToSearch = "b.AuthorName";
            cleanTerm = searchTerm.Substring(7).Trim();
        }
        else if (searchTerm.StartsWith("publisher:", StringComparison.OrdinalIgnoreCase))
        {
            columnToSearch = "b.Publisher";
            cleanTerm = searchTerm.Substring(10).Trim();
        }

        query += $" AND {columnToSearch} LIKE @searchTerm";
        parameters.Add(new MySqlParameter("@searchTerm", $"%{cleanTerm}%"));
    }

    if (categoryId.HasValue && categoryId.Value > 0)
    {
        query += " AND b.CategoryID = @categoryId";
        parameters.Add(new MySqlParameter("@categoryId", categoryId.Value));
    }

    if (!string.IsNullOrEmpty(availability))
    {
         if (availability == "available") query += " AND b.CopiesAvailable > 0";
         else if (availability == "unavailable") query += " AND b.CopiesAvailable = 0";
    }

    if (year.HasValue && year.Value > 0)
    {
        query += " AND b.Year = @year";
        parameters.Add(new MySqlParameter("@year", year.Value));
    }

    using (var cmd = new MySqlCommand(query, con))
    {
        cmd.Parameters.AddRange(parameters.ToArray());
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            books.Add(new BookModel
            {
                BookId = Convert.ToInt32(reader["BookId"]),
                BookName = reader["BookName"].ToString(),
                AuthorName = reader["AuthorName"].ToString(),
                // Use checking for DBNull to prevent crashes on optional fields
                AuthorID = reader["AuthorID"] != DBNull.Value ? Convert.ToInt32(reader["AuthorID"]) : 0,
                CopiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]),
                CopiesTotal = Convert.ToInt32(reader["CopiesTotal"]),
                Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                Publisher = reader["Publisher"].ToString(),
                BookDescription = reader["BookDescription"].ToString(),
                CategoryID = reader["CategoryID"] != DBNull.Value ? Convert.ToInt32(reader["CategoryID"]) : 0,
                CategoryName = reader["CategoryName"]?.ToString() ?? "Uncategorized",
                BookImage = reader["BookImage"] as byte[]
            });
        }
    }

    // --- PART B: FETCH CATEGORIES (Reusing 'con') ---
    using (var cmdCat = new MySqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", con))
    using (var readerCat = await cmdCat.ExecuteReaderAsync())
    {
        while (await readerCat.ReadAsync())
        {
            categories.Add(new
            {
                CategoryID = Convert.ToInt32(readerCat["CategoryID"]),
                CategoryName = readerCat["CategoryName"].ToString()
            });
        }
    }

    // --- PART C: FETCH YEARS (Reusing 'con') ---
    using (var cmdYear = new MySqlCommand("SELECT DISTINCT Year FROM Books WHERE Year IS NOT NULL ORDER BY Year DESC", con))
    using (var readerYear = await cmdYear.ExecuteReaderAsync())
    {
        while (await readerYear.ReadAsync())
        {
            years.Add(Convert.ToInt32(readerYear["Year"]));
        }
    }

    // Pass data to view
    ViewBag.SearchTerm = searchTerm; // Keep original term so input box doesn't change
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
        
        [HttpPost]
[Authorize]
public IActionResult AddReview(int BookId, string Content, int Rating)
{
    using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    con.Open();

    var query = @"INSERT INTO Reviews (BookId, UserName, Content, Rating, CreatedAt) 
                  VALUES (@BookId, @UserName, @Content, @Rating, NOW())";

    using var cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@BookId", BookId);
    cmd.Parameters.AddWithValue("@UserName", User.Identity.Name ?? "Anonymous");
    cmd.Parameters.AddWithValue("@Content", Content);
    cmd.Parameters.AddWithValue("@Rating", Rating);

    cmd.ExecuteNonQuery();

    return RedirectToAction("Details", new { id = BookId });
}

// ---------------------------------------------------------
// 3. ACTION: Moderator Panel (Only for Moderators)
// ---------------------------------------------------------
[Authorize(Roles = "moderator,admin")]
public IActionResult Reviews(int id)
{
    var reviews = new List<ReviewModel>();
    string bookName = "";

    using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    con.Open();

    // Get Book Name for display
    var nameCmd = new MySqlCommand("SELECT BookName FROM Books WHERE BookId = @id", con);
    nameCmd.Parameters.AddWithValue("@id", id);
    bookName = nameCmd.ExecuteScalar()?.ToString();

    // Get All Reviews
    var query = "SELECT * FROM Reviews WHERE BookId = @id ORDER BY CreatedAt DESC";
    using var cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", id);
    
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        reviews.Add(new ReviewModel
        {
            ReviewId = Convert.ToInt32(reader["ReviewId"]),
            BookId = id,
            UserName = reader["UserName"].ToString(),
            Content = reader["Content"].ToString(),
            Rating = Convert.ToInt32(reader["Rating"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
        });
    }

    ViewBag.BookName = bookName;
    ViewBag.BookId = id;
    return View(reviews);
}

// ---------------------------------------------------------
// 4. ACTION: Delete Review (Only for Moderators)
// ---------------------------------------------------------
[HttpPost]
[Authorize(Roles = "moderator,admin")]
public IActionResult DeleteReview(int reviewId, int bookId)
{
    using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
    con.Open();

    var query = "DELETE FROM Reviews WHERE ReviewId = @id";
    using var cmd = new MySqlCommand(query, con);
    cmd.Parameters.AddWithValue("@id", reviewId);
    
    cmd.ExecuteNonQuery();

    // Redirect back to the moderator panel
    return RedirectToAction("Reviews", new { id = bookId });
}

    }
}
