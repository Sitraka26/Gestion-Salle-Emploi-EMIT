namespace GestionSallesEmit.Models;

// Un "bloc" représente un cours affiché dans la grille (une case colorée qui peut
// s'étendre sur plusieurs heures, ex: 14h-18h).
public class BlocSeance
{
    public int HeureDebut { get; set; }      // ex : 14 pour "14h"
    public int DureeHeures { get; set; } = 1; // ex : 4 pour un cours de 14h à 18h
    public string Cours { get; set; } = string.Empty;
    public string Enseignant { get; set; } = string.Empty;
    public string Salle { get; set; } = string.Empty;
    public string Couleur { get; set; } = "#5fb3e8";
}

// Le modèle envoyé à la vue ParClasse.cshtml
public class PlanningClasseViewModel
{
    public string MentionNom { get; set; } = string.Empty;
    public string ParcoursNom { get; set; } = string.Empty;
    public string Niveau { get; set; } = string.Empty;

    public int HeureDebutGrille { get; set; } = 7;  // 7h
    public int HeureFinGrille { get; set; } = 18;   // 18h

    public string[] Jours { get; set; } = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };

    public Dictionary<string, List<BlocSeance>> BlocsParJour { get; set; } = new();
}
