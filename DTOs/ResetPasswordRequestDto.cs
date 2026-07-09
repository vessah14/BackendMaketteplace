using System.ComponentModel.DataAnnotations;

namespace Backend_Mboagreen.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }
    }

    public class ResetPasswordSubmitDto
    {
        [Required(ErrorMessage = "Token est obligatoire")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Nouveau mot de passe est obligatoire")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirmation du mot de passe est obligatoire")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; }
    }
}
