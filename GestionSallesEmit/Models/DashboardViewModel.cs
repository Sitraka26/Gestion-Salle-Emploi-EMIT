namespace GestionSallesEmit.Models;

public class DashboardViewModel
{
    public int TotalSalles { get; set; }
    public int TotalEnseignants { get; set; }
    public int TotalCours { get; set; }
    public int TotalSeances { get; set; }
    public double OccupancyRate { get; set; }
    public string? MostUsedRoom { get; set; }
    public string? BusiestDay { get; set; }
}
