namespace Backend_Mboagreen.Models
{
    public class VisitModel
    {
        public int Id { get; set; }

        // Date de la visite
        public DateTime VisitDate { get; set; } = DateTime.Now;


        // Optionnel : utilisateur connecté
        public int? UserId { get; set; }


        // Adresse IP du visiteur
        public string? IpAddress { get; set; }
    }
}