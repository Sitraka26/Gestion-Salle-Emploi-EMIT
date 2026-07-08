using GestionSallesEmit.Models;

namespace GestionSallesEmit.Data;

public static class SeedData
{
    public static void EnsureSeedData(ApplicationDbContext context)
    {
        // Salles
        if (!context.Salles.Any())
        {
            context.Salles.AddRange(new[] {
                new Salle { NomSalle = "A101", Capacite = 50 },
                new Salle { NomSalle = "B202", Capacite = 30 },
                new Salle { NomSalle = "Amphi1", Capacite = 120 }
            });
            context.SaveChanges();
        }

        // Enseignants
        if (!context.Enseignants.Any())
        {
            context.Enseignants.AddRange(new[] {
                new Enseignant { Nom = "Ben", Prenom = "Ahmed", Email = "ahmed.ben@example.com" },
                new Enseignant { Nom = "Diaz", Prenom = "Sarra", Email = "sarra.diaz@example.com" },
                new Enseignant { Nom = "Kamal", Prenom = "Omar", Email = "omar.kamal@example.com" }
            });
            context.SaveChanges();
        }

        // Cours
        if (!context.Cours.Any())
        {
            context.Cours.AddRange(new[] {
                new Cours { NomCours = "Mathématiques", Credit = 4 },
                new Cours { NomCours = "Programmation C#", Credit = 5 },
                new Cours { NomCours = "Bases de données", Credit = 3 }
            });
            context.SaveChanges();
        }
    }
}
