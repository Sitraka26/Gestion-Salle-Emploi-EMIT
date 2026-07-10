using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestionSallesEmit.Models;
using GestionSallesEmit.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionSallesEmit.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalSalles = await _context.Salles.CountAsync();
        var totalSeances = await _context.Seances.CountAsync();

        var topRoom = await _context.Seances
            .GroupBy(s => s.SalleId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync();
        var topRoomName = topRoom == null ? null : await _context.Salles
            .Where(s => s.Id == topRoom.Id)
            .Select(s => s.NomSalle)
            .FirstOrDefaultAsync();

        var busiestDay = await _context.Seances
            .GroupBy(s => s.Jour)
            .Select(g => new { Jour = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync();

        var sessionsByDay = await _context.Seances
            .GroupBy(s => s.Jour)
            .Select(g => new { Jour = g.Key, Count = g.Count() })
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalSalles = totalSalles,
            TotalEnseignants = await _context.Enseignants.CountAsync(),
            TotalCours = await _context.Cours.CountAsync(),
            TotalSeances = totalSeances,
            OccupancyRate = totalSalles > 0 ? Math.Round((double)totalSeances / totalSalles, 1) : 0,
            MostUsedRoom = topRoomName,
            BusiestDay = busiestDay?.Jour
        };

        ViewBag.ChartLabels = sessionsByDay.Select(d => d.Jour).ToArray();
        ViewBag.ChartData = sessionsByDay.Select(d => d.Count).ToArray();

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
