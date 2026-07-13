using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Enseignant
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Le nom must doit contenir entre 2 et 50 caractères.")]
    [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ\s\-']+$", ErrorMessage = "Le nom ne doit contenir que des lettres.")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Le prénom doit contenir entre 2 et 50 caractères.")]
    [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ\s\-']+$", ErrorMessage = "Le prénom ne doit contenir que des lettres.")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
    [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères.")]
    public string Email { get; set; } = string.Empty;
}