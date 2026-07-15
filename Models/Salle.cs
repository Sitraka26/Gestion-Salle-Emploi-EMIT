using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Salle
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom de la salle est obligatoire.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Le nom de la salle doit contenir entre 1 et 50 caractères.")]
    public string NomSalle { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "La capacité doit être comprise entre 1 et 1000 places.")]
    public int Capacite { get; set; }
}