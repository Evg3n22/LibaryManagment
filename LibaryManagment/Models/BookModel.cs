using System.ComponentModel.DataAnnotations; // This namespace is required for validation

namespace LibaryManagment.Models
{
    public class BookModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "The Book Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string BookName { get; set; }

        public string? AuthorName { get; set; }

        [Required(ErrorMessage = "Please select an author")]
        public int AuthorID { get; set; }

        [Required]
        [Range(0, 5000, ErrorMessage = "Copies must be a positive number")]
        public int CopiesAvailable { get; set; }

        [Required]
        [Range(0, 5000)]
        public int CopiesTotal { get; set; }

        [Required]
        [Range(1000, 2100, ErrorMessage = "Please enter a valid year (e.g., 1900-2100)")]
        public int Year { get; set; }

        [Required]
        [StringLength(100)]
        public string Publisher { get; set; }

        [StringLength(1000)]
        public string? BookDescription { get; set; }

        [Required]
        public int CategoryID { get; set; }
        
        public string? CategoryName { get; set; }

        public IFormFile? ImageFile { get; set; }
        public byte[]? BookImage { get; set; }
    }
}