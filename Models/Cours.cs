using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Cours
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du cours est obligatoire.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom du cours doit contenir entre 2 et 100 caractères.")]
    [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ\s\-'#+]+$", ErrorMessage = "Le nom du cours ne doit pas contenir de chiffres.")]
    public string NomCours { get; set; } = string.Empty;

    [Range(1, 30, ErrorMessage = "Les crédits doivent être compris entre 1 et 30.")]
    public int Credit { get; set; }

    // Niveau LMD fixe pour ce cours (ex: "Data Science" = toujours L3)
    [Required(ErrorMessage = "Le niveau est obligatoire.")]
    public string Niveau { get; set; } = "L1"; // L1, L2, L3, M1, M2

    public int? ParcoursId { get; set; }
    public Parcours? Parcours { get; set; }
}