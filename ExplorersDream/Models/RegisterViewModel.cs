using System.ComponentModel.DataAnnotations;


namespace ExplorersDream.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Полето е задължително")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Полето е задължително")] 
        public string LastName { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Password)]
        [Display(Name = "Потвърди паролата")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат. ")]
        public string ConfirmPassword { get; set; }
    }
}
