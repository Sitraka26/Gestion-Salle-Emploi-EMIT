using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Salle
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string NomSalle { get; set; } = string.Empty;

    public int Capacite { get; set; }
}