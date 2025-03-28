using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExplorersDream.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public bool UserStatus { get; set; } = true;

        [NotMapped] // Това казва на EF Core да не записва IsAdmin в базата
        public bool IsAdmin { get; set; }
    }
}
