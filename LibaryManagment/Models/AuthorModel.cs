using System.ComponentModel.DataAnnotations; // Required for validation

namespace LibaryManagment.Models
{
    public class AuthorModel
    {
        public int AuthorID { get; set; }

        [Required(ErrorMessage = "Author Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string AuthorName { get; set; }

        [Range(1000, 2100, ErrorMessage = "Please enter a valid birth year")]
        public int BirthYear { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Country name is too long")]
        public string Country { get; set; }

        [StringLength(1000)]
        public string AuthorDescription { get; set; }

        public IFormFile? ImageFile { get; set; }
        public byte[]? AuthorImage { get; set; }
    }
}