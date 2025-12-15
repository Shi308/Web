using System.ComponentModel.DataAnnotations;

namespace Lab2.Models
{
    public class MovieDTO
    {
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string? Title { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$")]
        [Required]
        [StringLength(30)]
        public string? Genre { get; set; }

        [Range(1, 100)]
        [Required]
        public decimal Price { get; set; }

        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        [StringLength(5)]
        [Required]
        public string? Rating { get; set; }
    }
}

