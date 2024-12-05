using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExplorersDream.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public List<OrderProduct> Products { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }
}
