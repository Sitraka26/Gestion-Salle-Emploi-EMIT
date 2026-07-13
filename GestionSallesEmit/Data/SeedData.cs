using GestionSallesEmit.Models;

namespace GestionSallesEmit.Data;

public static class SeedData
{
    public static void EnsureSeedData(ApplicationDbContext context)
    {
        if (!context.Salles.Any())
        {
            context.Salles.AddRange(new[] {
                new Salle { NomSalle = "A101", Capacite = 50 },
                new Salle { NomSalle = "B202", Capacite = 30 },
                new Salle { NomSalle = "Amphi1", Capacite = 120 }
            });
            context.SaveChanges();
        }

        if (!context.Enseignants.Any())
        {
            context.Enseignants.AddRange(new[] {
                new Enseignant { Nom = "Ben", Prenom = "Ahmed", Email = "ahmed.ben@example.com" },
                new Enseignant { Nom = "Diaz", Prenom = "Sarra", Email = "sarra.diaz@example.com" },
                new Enseignant { Nom = "Kamal", Prenom = "Omar", Email = "omar.kamal@example.com" }
            });
            context.SaveChanges();
        }

        if (!context.Mentions.Any())
        {
            context.Mentions.AddRange(new[] {
                new Mention { NomMention = "Informatique" },
                new Mention { NomMention = "Management" },
                new Mention { NomMention = "Communication" }
            });
            context.SaveChanges();
        }

        if (!context.Parcours.Any())
        {
            var mentionInfo = context.Mentions.First(m => m.NomMention == "Informatique");
            var mentionMgmt = context.Mentions.First(m => m.NomMention == "Management");
            var mentionCom = context.Mentions.First(m => m.NomMention == "Communication");

            context.Parcours.AddRange(new[] {
                new Parcours { NomParcours = "DA2I", MentionId = mentionInfo.Id },
                new Parcours { NomParcours = "AES", MentionId = mentionMgmt.Id },
                new Parcours { NomParcours = "ICM", MentionId = mentionCom.Id }
            });
            context.SaveChanges();
        }

        if (!context.Cours.Any())
        {
            var da2i = context.Parcours.First(p => p.NomParcours == "DA2I");
            context.Cours.AddRange(new[] {
                new Cours { NomCours = "Mathématiques", Credit = 4, ParcoursId = da2i.Id, Niveau = "L3" },
                new Cours { NomCours = "Programmation C#", Credit = 5, ParcoursId = da2i.Id, Niveau = "L3" },
                new Cours { NomCours = "Bases de données", Credit = 3, ParcoursId = da2i.Id, Niveau = "L3" }
            });
            context.SaveChanges();
        }
    }
}