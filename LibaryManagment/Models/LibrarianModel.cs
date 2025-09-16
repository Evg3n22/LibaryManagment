using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibaryManagment.Models;

public class LibrarianModel
{
    public int LibrarianId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    [BindNever] 
    public string Role { get; set; } = "lib";
}