using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibaryManagment.Models;

public class StudentModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    [BindNever] public string Role { get; set; } = "user";
}