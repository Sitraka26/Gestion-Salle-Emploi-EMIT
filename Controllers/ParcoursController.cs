using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class ParcoursController : Controller
{
    private readonly ApplicationDbContext _context;
    public ParcoursController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var parcours = await _context.Parcours.Include(p => p.Mention).Include(p => p.Cours).ToListAsync();
        return View(parcours);
    }

    public IActionResult Create()
    {
        ViewBag.Mentions = new SelectList(_context.Mentions, "Id", "NomMention");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NomParcours,MentionId")] Parcours parcours)
    {
        if (ModelState.IsValid)
        {
            _context.Add(parcours);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Parcours '{parcours.NomParcours}' ajouté.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Mentions = new SelectList(_context.Mentions, "Id", "NomMention", parcours.MentionId);
        return View(parcours);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var parcours = await _context.Parcours.Include(p => p.Mention).Include(p => p.Cours).FirstOrDefaultAsync(m => m.Id == id);
        if (parcours == null) return NotFound();
        return View(parcours);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var parcours = await _context.Parcours.Include(p => p.Cours).FirstOrDefaultAsync(m => m.Id == id);
        if (parcours != null)
        {
            if (parcours.Cours.Any())
            {
                TempData["ErrorMessage"] = "Impossible de supprimer : des cours sont rattachés à ce parcours.";
                return RedirectToAction(nameof(Index));
            }
            _context.Parcours.Remove(parcours);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Parcours supprimé.";
        }
        return RedirectToAction(nameof(Index));
    }
}