using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Seance : IValidatableObject
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le jour est obligatoire.")]
    public string Jour { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'heure de début est obligatoire.")]
    public string HeureDebut { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'heure de fin est obligatoire.")]
    public string HeureFin { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner une salle.")]
    public int SalleId { get; set; }
    public Salle? Salle { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un enseignant.")]
    public int EnseignantId { get; set; }
    public Enseignant? Enseignant { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un cours.")]
    public int CoursId { get; set; }
    public Cours? Cours { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TimeSpan.TryParse(HeureDebut, out var debut) && TimeSpan.TryParse(HeureFin, out var fin))
        {
            if (fin <= debut)
            {
                yield return new ValidationResult(
                    "L'heure de fin doit être postérieure à l'heure de début.",
                    new[] { nameof(HeureFin) });
            }
        }
    }
}