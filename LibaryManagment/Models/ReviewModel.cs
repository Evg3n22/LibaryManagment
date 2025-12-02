using System;
using System.ComponentModel.DataAnnotations;

namespace LibaryManagment.Models
{
    public class ReviewModel
    {
        [Key]
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public string UserName { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}