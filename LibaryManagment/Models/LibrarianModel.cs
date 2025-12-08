using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LibaryManagment.Models
{
    public class LibrarianModel
    {
        public int LibrarianId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [Range(18, 100, ErrorMessage = "Librarian must be between 18 and 100 years old")]
        public int Age { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [BindNever] 
        public string Role { get; set; } = "lib";
    }
}