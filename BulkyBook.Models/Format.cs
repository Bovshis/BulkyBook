using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Format
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Format")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
