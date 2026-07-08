using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class CoursController : Controller
{
    private readonly ApplicationDbContext _context;
    public CoursController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index() => View(await _context.Cours.ToListAsync());
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NomCours,Credit")] Cours cours)
    {
        if (ModelState.IsValid) { _context.Add(cours); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(cours);
    }
}