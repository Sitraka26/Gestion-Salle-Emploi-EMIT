using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Parcours
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du parcours est obligatoire.")]
    [StringLength(50)]
    public string NomParcours { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner une mention.")]
    public int MentionId { get; set; }
    public Mention? Mention { get; set; }

    public ICollection<Cours> Cours { get; set; } = new List<Cours>();
}