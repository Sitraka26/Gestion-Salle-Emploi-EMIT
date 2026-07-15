using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class MentionsController : Controller
{
    private readonly ApplicationDbContext _context;
    public MentionsController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var mentions = await _context.Mentions.Include(m => m.Parcours).ToListAsync();
        return View(mentions);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NomMention")] Mention mention)
    {
        if (ModelState.IsValid)
        {
            _context.Add(mention);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Mention '{mention.NomMention}' ajoutée.";
            return RedirectToAction(nameof(Index));
        }
        return View(mention);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var mention = await _context.Mentions.Include(m => m.Parcours).FirstOrDefaultAsync(m => m.Id == id);
        if (mention == null) return NotFound();
        return View(mention);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mention = await _context.Mentions.Include(m => m.Parcours).FirstOrDefaultAsync(m => m.Id == id);
        if (mention != null)
        {
            if (mention.Parcours.Any())
            {
                TempData["ErrorMessage"] = "Impossible de supprimer : des parcours sont rattachés à cette mention.";
                return RedirectToAction(nameof(Index));
            }
            _context.Mentions.Remove(mention);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mention supprimée.";
        }
        return RedirectToAction(nameof(Index));
    }
}