using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Enseignant
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public string Prenom { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}