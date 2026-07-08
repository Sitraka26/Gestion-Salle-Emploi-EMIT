using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;
using GestionSallesEmit.Hubs;

namespace GestionSallesEmit.Controllers;

public class SeancesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SeancesController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Affichage de l'emploi du temps (Données pour le calendrier)
    public async Task<IActionResult> Index()
    {
        var seances = await _context.Seances
            .Include(s => s.Salle)
            .Include(s => s.Enseignant)
            .Include(s => s.Cours)
            .ToListAsync();
        return View(seances);
    }

    // Retourne les séances au format JSON pour FullCalendar
    [HttpGet]
    public async Task<IActionResult> Events()
    {
        var seances = await _context.Seances
            .Include(s => s.Salle)
            .Include(s => s.Enseignant)
            .Include(s => s.Cours)
            .ToListAsync();

        var events = seances.Select(s => new {
            title = $"{s.Cours?.NomCours} - {s.Enseignant?.Nom} ({s.Salle?.NomSalle})",
            daysOfWeek = new[] { DayToNumber(s.Jour) },
            startTime = s.HeureDebut,
            endTime = s.HeureFin,
            backgroundColor = "#0d6efd"
        });

        return Json(events);
    }

    // Formulaire de planification (optionnellement prérempli via query string)
    public IActionResult Create(string? jour = null, string? debut = null, string? fin = null)
    {
        ViewBag.Salles = new SelectList(_context.Salles, "Id", "NomSalle");
        ViewBag.Enseignants = new SelectList(_context.Enseignants, "Id", "Nom");
        ViewBag.Cours = new SelectList(_context.Cours, "Id", "NomCours");

        var model = new Seance();
        if (!string.IsNullOrEmpty(jour)) model.Jour = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(jour.ToLower());
        if (!string.IsNullOrEmpty(debut)) model.HeureDebut = debut;
        if (!string.IsNullOrEmpty(fin)) model.HeureFin = fin;

        return View(model);
    }

    // Enregistrement sécurisé avec gestion des conflits
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Seance seance)
    {
        // Vérification si la salle OU l'enseignant est déjà pris au même moment le même jour
        var conflit = await _context.Seances
            .AnyAsync(s => s.Jour == seance.Jour 
                        && s.HeureDebut == seance.HeureDebut 
                        && (s.SalleId == seance.SalleId || s.EnseignantId == seance.EnseignantId));

        if (conflit)
        {
            ModelState.AddModelError("", "🚨 Alerte Conflit : La salle ou l'enseignant est déjà affecté à un cours sur ce créneau !");
            ViewBag.Salles = new SelectList(_context.Salles, "Id", "NomSalle", seance.SalleId);
            ViewBag.Enseignants = new SelectList(_context.Enseignants, "Id", "Nom", seance.EnseignantId);
            ViewBag.Cours = new SelectList(_context.Cours, "Id", "NomCours", seance.CoursId);
            return View(seance);
        }

        _context.Add(seance);
        await _context.SaveChangesAsync();

        // Notifier les clients connectés
        await _hubContext.Clients.All.SendAsync("SeanceCreated", new {
            id = seance.Id,
            jour = seance.Jour,
            heureDebut = seance.HeureDebut,
            heureFin = seance.HeureFin,
            salleId = seance.SalleId,
            enseignantId = seance.EnseignantId,
            coursId = seance.CoursId
        });

        return RedirectToAction(nameof(Index));
    }

    private int DayToNumber(string jour)
    {
        return jour?.ToLower() switch
        {
            "lundi" => 1,
            "mardi" => 2,
            "mercredi" => 3,
            "jeudi" => 4,
            "vendredi" => 5,
            "samedi" => 6,
            "dimanche" => 0,
            _ => 1
        };
    }
}