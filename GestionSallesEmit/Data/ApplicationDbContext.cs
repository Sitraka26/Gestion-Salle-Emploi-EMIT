using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Models; // Cette ligne est TRÈS importante pour trouver Salle, Enseignant et Cours

namespace GestionSallesEmit.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Salle> Salles { get; set; }
    public DbSet<Enseignant> Enseignants { get; set; }
    public DbSet<Cours> Cours { get; set; }
    public DbSet<Seance> Seances { get; set; }
}