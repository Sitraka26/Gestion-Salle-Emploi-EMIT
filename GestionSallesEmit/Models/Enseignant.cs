using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Enseignant
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères.")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Le format de l'email n'est pas valide (ex: nom@emit.mg).")]
    public string Email { get; set; } = string.Empty;
}