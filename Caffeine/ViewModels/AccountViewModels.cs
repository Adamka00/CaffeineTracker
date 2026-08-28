using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Felhasználónév kötelező!")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email cím kötelező!")]
        [EmailAddress(ErrorMessage = "Érvénytelen email cím!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jelszó kötelező!")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A jelszó legalább 6 karakter legyen!")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email cím kötelező!")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jelszó kötelező!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}