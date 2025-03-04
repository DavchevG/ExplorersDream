using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExplorersDream.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(100, ErrorMessage = "Името не може да надвишава 100 символа")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "караткото описание е задължително.")]
        [StringLength(50, ErrorMessage = "Описанието не може да надвишава 50 символа.")]
        public string? ShortDescription {  get; set; }

        [Required(ErrorMessage = "Описанието е задължително.")]
        [StringLength(500, ErrorMessage = "Описанието не може да надвишава 500 символа.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цената е задължителна.")]
        [Range(0.01, 10000, ErrorMessage = "Цената трябва да бъде между 0.01 и 10,000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Името на марката е задължително")]
        [StringLength(50, ErrorMessage = "Марка не може да надвишава 50 символа.")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна.")]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        [ValidateNever]
        public Category? Category { get; set; }
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
    