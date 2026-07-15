using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Models;

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
    public DbSet<Mention> Mentions { get; set; }
    public DbSet<Parcours> Parcours { get; set; }
    public DbSet<Indisponibilite> Indisponibilites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Parcours>()
            .HasOne(p => p.Mention)
            .WithMany(m => m.Parcours)
            .HasForeignKey(p => p.MentionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cours>()
            .HasOne(c => c.Parcours)
            .WithMany(p => p.Cours)
            .HasForeignKey(c => c.ParcoursId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Indisponibilite>()
            .HasOne(i => i.Enseignant)
            .WithMany(e => e.Indisponibilites)
            .HasForeignKey(i => i.EnseignantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}