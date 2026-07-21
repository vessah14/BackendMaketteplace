using System.ComponentModel.DataAnnotations;

namespace Backend_Mboagreen.Models
{
    public class UserModel
    {
        public int Id { get; set; } // AUTO INCREMENT

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Telephone { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Adresse { get; set; }
        public string? Ville { get; set; }
        public string? Quartier { get; set; }

        public bool AcceptTerms { get; set; }
        public bool Newsletter { get; set; }
        public DateTime CreatedAt { get; set; }

        // IMPORTANT pour ton radio bouton
        public string UserType { get; set; } // vendeur / acheteur
        public ICollection<AnnonceModel> Annonce { get; set; } = new List<AnnonceModel>();
    }
}