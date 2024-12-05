using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExplorersDream.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public List<Order> Orders { get; set; }
    }
}
