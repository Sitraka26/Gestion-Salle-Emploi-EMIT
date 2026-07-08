using System.ComponentModel.DataAnnotations;

namespace GestionSallesEmit.Models;

public class Seance
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Jour { get; set; } = string.Empty; // Lundi, Mardi, etc.

    [Required]
    public string HeureDebut { get; set; } = string.Empty; // Format "HH:mm" (ex: "08:00")

    [Required]
    public string HeureFin { get; set; } = string.Empty; // Format "HH:mm" (ex: "10:00")

    // Clés étrangères vers tes tables existantes
    public int SalleId { get; set; }
    public Salle? Salle { get; set; }

    public int EnseignantId { get; set; }
    public Enseignant? Enseignant { get; set; }

    public int CoursId { get; set; }
    public Cours? Cours { get; set; }
}