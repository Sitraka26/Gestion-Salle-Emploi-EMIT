using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Cours
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string NomCours { get; set; } = string.Empty;

    public int Credit { get; set; }
}