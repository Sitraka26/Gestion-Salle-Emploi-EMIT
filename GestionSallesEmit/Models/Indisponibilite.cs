using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Indisponibilite
{
    [Key]
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un enseignant.")]
    public int EnseignantId { get; set; }
    public Enseignant? Enseignant { get; set; }

    [Required(ErrorMessage = "Le jour est obligatoire.")]
    public string Jour { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'heure de début est obligatoire.")]
    public string HeureDebut { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'heure de fin est obligatoire.")]
    public string HeureFin { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Raison { get; set; }
}