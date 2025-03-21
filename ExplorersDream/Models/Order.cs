using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExplorersDream.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Трябва да е string, защото Identity използва string като Id

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public List<OrderProduct> Products { get; set; } = new List<OrderProduct>();

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Начален статус
    }
}
