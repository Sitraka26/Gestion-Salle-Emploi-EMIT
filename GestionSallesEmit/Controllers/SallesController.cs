using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class SallesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SallesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Salles (Pour afficher la liste)
    public async Task<IActionResult> Index()
    {
        var salles = await _context.Salles.ToListAsync();
        return View(salles);
    }

    // GET: Salles/Create (Pour afficher le formulaire)
    public IActionResult Create()
    {
        return View();
    }

    // POST: Salles/Create (Pour enregistrer la salle dans PostgreSQL)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NomSalle,Capacite")] Salle salle)
    {
        if (ModelState.IsValid)
        {
            _context.Add(salle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(salle);
    }
}