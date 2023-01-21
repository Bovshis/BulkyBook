using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Display order")]
        [Range(1,500,ErrorMessage = "Display Order must be between 1 and 500 only!")]
        public int DisplayOrder { get; set; }
        [ValidateNever]
        public IList<SubCategory> SubCategories { get; set; }
    }
}
