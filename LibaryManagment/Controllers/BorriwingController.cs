using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using LibaryManagment.Models;
using Microsoft.AspNetCore.Authorization;

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
            con.Open();

            MySqlCommand cmd;

            if (User.IsInRole("admin") || User.IsInRole("lib"))
            {
                cmd = new MySqlCommand("SELECT * FROM Borrowings", con);
            }
            else if (User.IsInRole("Student"))
            {
                cmd = new MySqlCommand("SELECT * FROM Borrowings WHERE StudentName = @studentName", con);
                cmd.Parameters.AddWithValue("@studentName", User.Identity.Name); 
            }
            else
            {
                return Forbid();
            }

            using (var reader = cmd.ExecuteReader())  // Changed to using block
            {
                while (reader.Read())
                {
                    list.Add(new BorrowingModel
                    {
                        BorrowingId = Convert.ToInt32(reader["BorrowingId"]),
                        StudentName = reader["StudentName"].ToString(),
                        BookName = reader["BookName"].ToString(),
                        BorrowDate = Convert.ToDateTime(reader["BorrowDate"]),
                        ReturnedDate = reader["ReturnedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnedDate"]) : (DateTime?)null
                    });
                }
            } // Reader is closed here

            // Now we can execute another query on the same connection
            if (User.IsInRole("Student"))
            {
                List<QueueModel> userQueue = new();
                var queueCmd = new MySqlCommand(
                    "SELECT * FROM BookQueue WHERE StudentName = @studentName ORDER BY RequestDate", con);
                queueCmd.Parameters.AddWithValue("@studentName", User.Identity.Name);
                
                using (var queueReader = queueCmd.ExecuteReader())
                {
                    while (queueReader.Read())
                    {
                        userQueue.Add(new QueueModel
                        {
                            QueueId = Convert.ToInt32(queueReader["QueueId"]),
                            StudentName = queueReader["StudentName"].ToString(),
                            BookName = queueReader["BookName"].ToString(),
                            RequestDate = Convert.ToDateTime(queueReader["RequestDate"])
                        });
                    }
                }
                ViewBag.UserQueue = userQueue;
            }

            return View(list);
        }

        [Authorize(Roles = "admin,lib")]
        [HttpGet]
        public IActionResult Create()
        {
            List<string> students = new();
            List<string> books = new();
            Dictionary<string, int> bookAvailability = new();

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var studentCmd = new MySqlCommand("SELECT StudentName FROM Students", con);
            using (var reader = studentCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    students.Add(reader["StudentName"].ToString());
                }
            }

            var bookCmd = new MySqlCommand("SELECT BookName, CopiesAvailable FROM Books", con);
            using (var reader = bookCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string bookName = reader["BookName"].ToString();
                    int available = Convert.ToInt32(reader["CopiesAvailable"]);
                    books.Add(bookName);
                    bookAvailability[bookName] = available;
                }
            }

            ViewBag.Students = students;
            ViewBag.Books = books;
            ViewBag.BookAvailability = bookAvailability;

            return View();
        }

        [HttpPost]
        public IActionResult Create(BorrowingModel model)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Перевіряємо доступність копій
            var checkCmd = new MySqlCommand(
                "SELECT CopiesTotal, CopiesAvailable FROM Books WHERE BookName = @bookname", con);
            checkCmd.Parameters.AddWithValue("@bookname", model.BookName);
            
            int copiesTotal = 0;
            int copiesAvailable = 0;
            
            using (var reader = checkCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    copiesTotal = Convert.ToInt32(reader["CopiesTotal"]);
                    copiesAvailable = Convert.ToInt32(reader["CopiesAvailable"]);
                }
            }

            if (copiesAvailable > 0)
            {
                // Є доступні копії - створюємо Borrowing
                var cmd = new MySqlCommand(
                    "INSERT INTO Borrowings (StudentName, BookName, BorrowDate, ReturnedDate) VALUES (@studentname, @bookname, @borrowdate, @returneddate)",
                    con);
                cmd.Parameters.AddWithValue("@studentname", model.StudentName);
                cmd.Parameters.AddWithValue("@bookname", model.BookName);
                cmd.Parameters.AddWithValue("@borrowdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@returneddate", DBNull.Value);
                cmd.ExecuteNonQuery();

                // Зменшуємо кількість доступних копій
                var updateBook = new MySqlCommand(
                    "UPDATE Books SET CopiesAvailable = CopiesAvailable - 1 WHERE BookName = @bookname", con);
                updateBook.Parameters.AddWithValue("@bookname", model.BookName);
                updateBook.ExecuteNonQuery();

                TempData["Success"] = "Книгу успішно видано студенту.";
            }
            else
            {
                // Немає доступних копій - додаємо в чергу
                var queueCmd = new MySqlCommand(
                    "INSERT INTO BookQueue (StudentName, BookName, RequestDate) VALUES (@studentname, @bookname, @requestdate)",
                    con);
                queueCmd.Parameters.AddWithValue("@studentname", model.StudentName);
                queueCmd.Parameters.AddWithValue("@bookname", model.BookName);
                queueCmd.Parameters.AddWithValue("@requestdate", DateTime.Now);
                queueCmd.ExecuteNonQuery();

                TempData["Info"] = "Всі копії зайняті. Студента додано в чергу очікування.";
            }

            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "admin,lib")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            BorrowingModel model = new();

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

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
                    model.ReturnedDate = reader["ReturnedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnedDate"]) : (DateTime?)null;
                }
            }

            List<string> students = new();
            var studentCmd = new MySqlCommand("SELECT StudentName FROM Students", con);
            using (var studentReader = studentCmd.ExecuteReader())
            {
                while (studentReader.Read())
                {
                    students.Add(studentReader["StudentName"].ToString());
                }
            }

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
            con.Open();

            // Отримуємо попередній стан
            var getOldCmd = new MySqlCommand("SELECT ReturnedDate, BookName FROM Borrowings WHERE BorrowingId=@id", con);
            getOldCmd.Parameters.AddWithValue("@id", model.BorrowingId);
            
            DateTime? oldReturnedDate = null;
            string oldBookName = "";
            
            using (var reader = getOldCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    oldReturnedDate = reader["ReturnedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnedDate"]) : (DateTime?)null;
                    oldBookName = reader["BookName"].ToString();
                }
            }

            // Оновлюємо Borrowing
            var cmd = new MySqlCommand(
                "UPDATE Borrowings SET StudentName=@studentname, BookName=@bookname, BorrowDate=@borrowdate, ReturnedDate=@returneddate WHERE BorrowingId=@id",
                con);
            cmd.Parameters.AddWithValue("@studentname", model.StudentName);
            cmd.Parameters.AddWithValue("@bookname", model.BookName);
            cmd.Parameters.AddWithValue("@borrowdate", model.BorrowDate);
            cmd.Parameters.AddWithValue("@returneddate", model.ReturnedDate.HasValue ? (object)model.ReturnedDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", model.BorrowingId);
            cmd.ExecuteNonQuery();

            // Якщо книга щойно повернута (була не повернута, стала повернута)
            if (!oldReturnedDate.HasValue && model.ReturnedDate.HasValue)
            {
                // Збільшуємо доступні копії
                var updateBook = new MySqlCommand(
                    "UPDATE Books SET CopiesAvailable = CopiesAvailable + 1 WHERE BookName = @bookname", con);
                updateBook.Parameters.AddWithValue("@bookname", model.BookName);
                updateBook.ExecuteNonQuery();

                // Перевіряємо чергу
                ProcessQueue(con, model.BookName);
                
                TempData["Success"] = "Книгу повернуто успішно!";
            }

            return RedirectToAction("Index");
        }

        // Швидке повернення книги
        [Authorize(Roles = "admin,lib")]
        public IActionResult Return(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            // Отримуємо інформацію про позику
            string bookName = "";
            var getCmd = new MySqlCommand("SELECT BookName, ReturnedDate FROM Borrowings WHERE BorrowingId=@id", con);
            getCmd.Parameters.AddWithValue("@id", id);
            
            using (var reader = getCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    bookName = reader["BookName"].ToString();
                    if (reader["ReturnedDate"] != DBNull.Value)
                    {
                        TempData["Warning"] = "Ця книга вже була повернута!";
                        return RedirectToAction("Index");
                    }
                }
            }

            // Встановлюємо дату повернення
            var updateCmd = new MySqlCommand(
                "UPDATE Borrowings SET ReturnedDate = @returndate WHERE BorrowingId = @id", con);
            updateCmd.Parameters.AddWithValue("@returndate", DateTime.Now);
            updateCmd.Parameters.AddWithValue("@id", id);
            updateCmd.ExecuteNonQuery();

            // Збільшуємо доступні копії
            var updateBook = new MySqlCommand(
                "UPDATE Books SET CopiesAvailable = CopiesAvailable + 1 WHERE BookName = @bookname", con);
            updateBook.Parameters.AddWithValue("@bookname", bookName);
            updateBook.ExecuteNonQuery();

            // Перевіряємо чергу
            ProcessQueue(con, bookName);

            TempData["Success"] = "Книгу повернуто успішно!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin,lib")]
        public IActionResult Delete(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection")); 
            con.Open();

            string bookName = "";
            DateTime? returnedDate = null;
            
            var getBookCmd = new MySqlCommand("SELECT BookName, ReturnedDate FROM Borrowings WHERE BorrowingId=@id", con);
            getBookCmd.Parameters.AddWithValue("@id", id);
            using (var reader = getBookCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    bookName = reader["BookName"].ToString();
                    returnedDate = reader["ReturnedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnedDate"]) : (DateTime?)null;
                }
            }

            var cmd = new MySqlCommand("DELETE FROM Borrowings WHERE BorrowingId=@id", con); 
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            // Якщо книга не була повернута - звільняємо копію
            if (!string.IsNullOrEmpty(bookName) && !returnedDate.HasValue)
            {
                var updateBook = new MySqlCommand(
                    "UPDATE Books SET CopiesAvailable = CopiesAvailable + 1 WHERE BookName=@bookname", con);
                updateBook.Parameters.AddWithValue("@bookname", bookName);
                updateBook.ExecuteNonQuery();

                // Перевіряємо чергу
                ProcessQueue(con, bookName);
            }

            return RedirectToAction("Index");
        }

        // Метод для обробки черги
        private void ProcessQueue(MySqlConnection con, string bookName)
        {
            // Перевіряємо, чи є хтось у черзі на цю книгу
            var queueCmd = new MySqlCommand(
                "SELECT QueueId, StudentName FROM BookQueue WHERE BookName = @bookname ORDER BY RequestDate LIMIT 1", con);
            queueCmd.Parameters.AddWithValue("@bookname", bookName);

            int queueId = 0;
            string studentName = "";

            using (var reader = queueCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    queueId = Convert.ToInt32(reader["QueueId"]);
                    studentName = reader["StudentName"].ToString();
                }
            }

            if (queueId > 0)
            {
                // Є студент у черзі - автоматично видаємо йому книгу
                var borrowCmd = new MySqlCommand(
                    "INSERT INTO Borrowings (StudentName, BookName, BorrowDate, ReturnedDate) VALUES (@studentname, @bookname, @borrowdate, @returneddate)",
                    con);
                borrowCmd.Parameters.AddWithValue("@studentname", studentName);
                borrowCmd.Parameters.AddWithValue("@bookname", bookName);
                borrowCmd.Parameters.AddWithValue("@borrowdate", DateTime.Now);
                borrowCmd.Parameters.AddWithValue("@returneddate", DBNull.Value);
                borrowCmd.ExecuteNonQuery();

                // Зменшуємо доступні копії назад
                var updateBook = new MySqlCommand(
                    "UPDATE Books SET CopiesAvailable = CopiesAvailable - 1 WHERE BookName = @bookname", con);
                updateBook.Parameters.AddWithValue("@bookname", bookName);
                updateBook.ExecuteNonQuery();

                // Видаляємо зі черги
                var deleteQueue = new MySqlCommand("DELETE FROM BookQueue WHERE QueueId = @queueid", con);
                deleteQueue.Parameters.AddWithValue("@queueid", queueId);
                deleteQueue.ExecuteNonQuery();
            }
        }

        // Показати чергу (для адміна/бібліотекаря)
        [Authorize(Roles = "admin,lib")]
        public IActionResult Queue()
        {
            List<QueueModel> queue = new();
            Dictionary<string, int> queuePositions = new();
            
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand("SELECT * FROM BookQueue ORDER BY BookName, RequestDate", con);
            using var reader = cmd.ExecuteReader();
            
            string currentBook = "";
            int position = 0;
            
            while (reader.Read())
            {
                string bookName = reader["BookName"].ToString();
                
                // Скидаємо позицію для нової книги
                if (bookName != currentBook)
                {
                    currentBook = bookName;
                    position = 1;
                }
                else
                {
                    position++;
                }
                
                int queueId = Convert.ToInt32(reader["QueueId"]);
                queuePositions[queueId.ToString()] = position;
                
                queue.Add(new QueueModel
                {
                    QueueId = queueId,
                    StudentName = reader["StudentName"].ToString(),
                    BookName = bookName,
                    RequestDate = Convert.ToDateTime(reader["RequestDate"])
                });
            }

            ViewBag.QueuePositions = queuePositions;
            return View(queue);
        }

        // Видалити зі черги
        [Authorize(Roles = "admin,lib")]
        public IActionResult RemoveFromQueue(int id)
        {
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand("DELETE FROM BookQueue WHERE QueueId = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Queue");
        }
    }
}