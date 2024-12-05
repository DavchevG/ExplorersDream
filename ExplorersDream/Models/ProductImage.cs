using ExplorersDream.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourNamespace.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
