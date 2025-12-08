using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibaryManagment.Models
{
    public class StudentModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string StudentName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")] // Validates email format
        public string Email { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid Phone Number")] // Validates phone format
        public string Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [BindNever] 
        public string Role { get; set; } = "user";
    }
}