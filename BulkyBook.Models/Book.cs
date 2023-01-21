using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyBook.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [ValidateNever]
        public string? ImageUrl { get; set; }

        [Required]
        [DisplayName("Subcategory")]
        public int SubCategoryId { get; set; }
        [ForeignKey("SubCategoryId")]
        [ValidateNever]
        public SubCategory? SubCategory { get; set; }

        [ValidateNever]
        public IEnumerable<Product> Products { get; set; }

        [NotMapped]
        [ValidateNever]
        public string DataTextFieldLabel => $"{Title} ({Author})";
    }
}
