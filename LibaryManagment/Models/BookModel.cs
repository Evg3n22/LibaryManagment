namespace LibaryManagment.Models;

public class BookModel
{
    public int BookId { get; set; }
    public string BookName { get; set; }
    public string Author { get; set; }
    public bool Available { get; set; }
}