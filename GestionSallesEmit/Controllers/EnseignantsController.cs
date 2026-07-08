using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class EnseignantsController : Controller
{
    private readonly ApplicationDbContext _context;
    public EnseignantsController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index() => View(await _context.Enseignants.ToListAsync());
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email")] Enseignant enseignant)
    {
        if (ModelState.IsValid) { _context.Add(enseignant); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(enseignant);
    }
}