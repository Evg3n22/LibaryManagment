namespace LibaryManagment.Models;

public class BookModel
{
    public int BookId { get; set; }
    public string BookName { get; set; }
    public string AuthorName { get; set; }
    public int AuthorID { get; set; }
    public int CopiesAvailable { get; set; }
    public int CopiesTotal { get; set; }
    public int Year { get; set; }
    public string Publisher { get; set; }
    public string BookDescription { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    
    public IFormFile? ImageFile { get; set; }
    public byte[] BookImage { get; set; }
    /*public bool Available { get; set; }*/
}