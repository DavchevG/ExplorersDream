using System.ComponentModel.DataAnnotations;

namespace ExplorersDream.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Полето е задължително")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
        public string Password { get; set; }

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }    
    }
}
