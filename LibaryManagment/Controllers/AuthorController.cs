using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibaryManagment.Controllers
{
    public class AuthorController : Controller
    {
        private readonly IConfiguration _config;

        public AuthorController(IConfiguration config)
        {
            _config = config;
        }

        // GET: List of authors
        public IActionResult Index()
        {
            var authors = new List<AuthorModel>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Author", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                authors.Add(new AuthorModel
                {
                    AuthorID = Convert.ToInt32(reader["AuthorID"]),
                    AuthorName = reader["AuthorName"].ToString(),
                    BirthYear = Convert.ToInt32(reader["BirthYear"]),
                    Country = reader["Country"].ToString(),
                    AuthorDescription = reader["AuthorDescription"].ToString(),
                    AuthorImage = reader["AuthorImage"] as byte[]
                });
            }
            return View(authors);
        }

        // GET: Create author
        [Authorize(Roles = "admin,lib")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create author
        [HttpPost]
        [Authorize(Roles = "admin,lib")]
        public async Task<IActionResult> Create(AuthorModel model)
        {
            // Handle the uploaded image file
            if (model.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                model.AuthorImage = memoryStream.ToArray();
            }

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand(
                @"INSERT INTO Author 
                (AuthorName, BirthYear, Country, AuthorDescription, AuthorImage) 
                VALUES 
                (@AuthorName, @BirthYear, @Country, @AuthorDescription, @AuthorImage)", con);
            
            cmd.Parameters.AddWithValue("@AuthorName", model.AuthorName);
            cmd.Parameters.AddWithValue("@BirthYear", model.BirthYear);
            cmd.Parameters.AddWithValue("@Country", model.Country);
            cmd.Parameters.AddWithValue("@AuthorDescription", model.AuthorDescription);
            cmd.Parameters.AddWithValue("@AuthorImage", model.AuthorImage ?? (object)DBNull.Value);
            
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
        
        // Add this method to your AuthorController class

        // GET: Author Details
        public IActionResult Details(int id)
        {
            AuthorModel author = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Author WHERE AuthorID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
    
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                author.AuthorID = Convert.ToInt32(reader["AuthorID"]);
                author.AuthorName = reader["AuthorName"].ToString();
                author.BirthYear = Convert.ToInt32(reader["BirthYear"]);
                author.Country = reader["Country"].ToString();
                author.AuthorDescription = reader["AuthorDescription"].ToString();
                author.AuthorImage = reader["AuthorImage"] as byte[];
            }
    
            // Get books by this author
            var books = new List<BookModel>();
            using (var con2 = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var cmdBooks = new MySqlCommand(
                    @"SELECT BookId, BookName, Year, Publisher, CopiesAvailable 
              FROM Books WHERE AuthorID = @authorId 
              ORDER BY Year DESC", con2);
                cmdBooks.Parameters.AddWithValue("@authorId", id);
                con2.Open();
        
                using var readerBooks = cmdBooks.ExecuteReader();
                while (readerBooks.Read())
                {
                    books.Add(new BookModel
                    {
                        BookId = Convert.ToInt32(readerBooks["BookId"]),
                        BookName = readerBooks["BookName"].ToString(),
                        Year = Convert.ToInt32(readerBooks["Year"]),
                        Publisher = readerBooks["Publisher"].ToString(),
                        CopiesAvailable = Convert.ToInt32(readerBooks["CopiesAvailable"])
                    });
                }
            }
    
            ViewBag.Books = books;
            return View(author);
        }

        // GET: Edit author
        [Authorize(Roles = "admin,lib")]
        public IActionResult Edit(int id)
        {
            AuthorModel author = new();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Author WHERE AuthorID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                author.AuthorID = Convert.ToInt32(reader["AuthorID"]);
                author.AuthorName = reader["AuthorName"].ToString();
                author.BirthYear = Convert.ToInt32(reader["BirthYear"]);
                author.Country = reader["Country"].ToString();
                author.AuthorDescription = reader["AuthorDescription"].ToString();
                author.AuthorImage = reader["AuthorImage"] as byte[];
            }
            return View(author);
        }

        // POST: Edit author
        [HttpPost]
        [Authorize(Roles = "admin,lib")]
        public async Task<IActionResult> Edit(AuthorModel model)
        {
            // Handle the uploaded image file
            if (model.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await model.ImageFile.CopyToAsync(memoryStream);
                model.AuthorImage = memoryStream.ToArray();
            }

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand(
                @"UPDATE Author SET 
                AuthorName=@AuthorName, BirthYear=@BirthYear, Country=@Country, 
                AuthorDescription=@AuthorDescription, AuthorImage=@AuthorImage
                WHERE AuthorID=@id", con);
            
            cmd.Parameters.AddWithValue("@AuthorName", model.AuthorName);
            cmd.Parameters.AddWithValue("@BirthYear", model.BirthYear);
            cmd.Parameters.AddWithValue("@Country", model.Country);
            cmd.Parameters.AddWithValue("@AuthorDescription", model.AuthorDescription);
            cmd.Parameters.AddWithValue("@AuthorImage", model.AuthorImage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", model.AuthorID);
            
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        // GET: Delete author
        [Authorize(Roles = "admin,lib")]
        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("DELETE FROM Author WHERE AuthorID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
//
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using LibaryManagment.Models;
// using LibaryManagment.Data;
// using Microsoft.AspNetCore.Authorization;
//
// namespace LibaryManagment.Controllers
// {
//     public class AuthorController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//
//         public AuthorController(ApplicationDbContext context)
//         {
//             _context = context;
//         }
//
//         public async Task<IActionResult> Index()
//         {
//             var authors = await _context.Authors.ToListAsync();
//             return View(authors);
//         }
//
//         [Authorize(Roles = "admin,lib")]
//         public IActionResult Create()
//         {
//             return View();
//         }
//
//         [HttpPost]
//         public async Task<IActionResult> Create(AuthorModel author)
//         {
//             if (!ModelState.IsValid)
//                 return View(author);
//
//             // Handle image upload
//             if (author.ImageFile != null)
//             {
//                 using var memoryStream = new MemoryStream();
//                 await author.ImageFile.CopyToAsync(memoryStream);
//                 author.AuthorImage = memoryStream.ToArray();
//             }
//
//             _context.Authors.Add(author);
//             await _context.SaveChangesAsync();
//
//             return RedirectToAction("Index");
//         }
//
//         [Authorize(Roles = "admin,lib")]
//         public async Task<IActionResult> Edit(int id)
//         {
//             var author = await _context.Authors.FindAsync(id);
//             if (author == null)
//                 return NotFound();
//
//             return View(author);
//         }
//
//         [HttpPost]
//         public async Task<IActionResult> Edit(AuthorModel author)
//         {
//             if (!ModelState.IsValid)
//                 return View(author);
//
//             // Handle image upload if new image provided
//             if (author.ImageFile != null)
//             {
//                 using var memoryStream = new MemoryStream();
//                 await author.ImageFile.CopyToAsync(memoryStream);
//                 author.AuthorImage = memoryStream.ToArray();
//             }
//
//             _context.Authors.Update(author);
//             await _context.SaveChangesAsync();
//
//             return RedirectToAction("Index");
//         }
//
//         [Authorize(Roles = "admin,lib")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var author = await _context.Authors.FindAsync(id);
//             if (author != null)
//             {
//                 _context.Authors.Remove(author);
//                 await _context.SaveChangesAsync();
//             }
//             return RedirectToAction("Index");
//         }
//     }
// }