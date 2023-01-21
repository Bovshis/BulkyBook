using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key] 
        public int Id { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public double SalePrice { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        [DisplayName("Book")]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        [ValidateNever]
        public Book Book { get; set; }
        [Required]
        [DisplayName("Format")]
        public int FormatId { get; set; }
        [ForeignKey("FormatId")]
        [ValidateNever]
        public Format Format { get; set; }
    }
}
