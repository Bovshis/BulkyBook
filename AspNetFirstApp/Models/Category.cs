using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspNetFirstApp.Models
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
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
