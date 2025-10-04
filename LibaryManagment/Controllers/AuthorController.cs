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