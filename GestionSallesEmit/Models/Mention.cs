using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Mention
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom de la mention est obligatoire.")]
    [StringLength(100)]
    public string NomMention { get; set; } = string.Empty;

    public ICollection<Parcours> Parcours { get; set; } = new List<Parcours>();
}