using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class IndisponibilitesController : Controller
{
    private readonly ApplicationDbContext _context;
    public IndisponibilitesController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(int enseignantId)
    {
        var enseignant = await _context.Enseignants.FindAsync(enseignantId);
        if (enseignant == null) return NotFound();

        ViewBag.Enseignant = enseignant;
        var indisponibilites = await _context.Indisponibilites
            .Where(i => i.EnseignantId == enseignantId)
            .ToListAsync();
        return View(indisponibilites);
    }

    public IActionResult Create(int enseignantId)
    {
        ViewBag.EnseignantId = enseignantId;
        return View(new Indisponibilite { EnseignantId = enseignantId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,EnseignantId,Jour,HeureDebut,HeureFin,Raison")] Indisponibilite indisponibilite)
    {
        if (ModelState.IsValid)
        {
            _context.Add(indisponibilite);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Indisponibilité ajoutée.";
            return RedirectToAction(nameof(Index), new { enseignantId = indisponibilite.EnseignantId });
        }
        ViewBag.EnseignantId = indisponibilite.EnseignantId;
        return View(indisponibilite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var indispo = await _context.Indisponibilites.FindAsync(id);
        if (indispo != null)
        {
            int enseignantId = indispo.EnseignantId;
            _context.Indisponibilites.Remove(indispo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Indisponibilité supprimée.";
            return RedirectToAction(nameof(Index), new { enseignantId });
        }
        return RedirectToAction("Index", "Enseignants");
    }
}