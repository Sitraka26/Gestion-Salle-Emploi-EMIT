using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class EnseignantsController : Controller
{
    private readonly ApplicationDbContext _context;
    public EnseignantsController(ApplicationDbContext context) { _context = context; }

    public IActionResult Index() => View();
    public IActionResult Create() => View();

    [HttpGet]
    public async Task<IActionResult> ListJson(string q = "", string sort = "name", int page = 1, int pageSize = 10, int minClasses = 0)
    {
        var query = _context.Enseignants.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(e => e.Nom.Contains(q) || e.Prenom.Contains(q) || e.Email.Contains(q));

        if (minClasses > 0)
        {
            var busyIds = await _context.Seances
                .GroupBy(s => s.EnseignantId)
                .Where(g => g.Count() >= minClasses)
                .Select(g => g.Key)
                .ToListAsync();
            query = query.Where(e => busyIds.Contains(e.Id));
        }

        var total = await query.CountAsync();
        query = sort switch
        {
            "email" => query.OrderBy(e => e.Email),
            "classes" => query.OrderByDescending(e => _context.Seances.Count(s => s.EnseignantId == e.Id)),
            _ => query.OrderBy(e => e.Nom),
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.Nom,
                e.Prenom,
                e.Email,
                sessionCount = _context.Seances.Count(s => s.EnseignantId == e.Id)
            })
            .ToListAsync();

        return Json(new { items, total });
    }

    [HttpGet]
    public async Task<IActionResult> SummaryJson(int minClasses = 0)
    {
        var totalTeachers = await _context.Enseignants.CountAsync();
        var totalSessions = await _context.Seances.CountAsync();
        var activeTeachers = await _context.Seances.Select(s => s.EnseignantId).Distinct().CountAsync();
        var averageClasses = totalTeachers > 0 ? Math.Round(totalSessions / (double)totalTeachers, 1) : 0;

        var busiest = await _context.Seances
            .GroupBy(s => s.EnseignantId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync();

        var busiestTeacher = busiest == null ? null : await _context.Enseignants
            .Where(e => e.Id == busiest.Id)
            .Select(e => new { fullName = e.Nom + " " + e.Prenom, busiest.Count })
            .FirstOrDefaultAsync();

        return Json(new
        {
            totalTeachers,
            activeTeachers,
            averageClasses,
            busiestTeacher
        });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var enseignant = await _context.Enseignants.FindAsync(id.Value);
        if (enseignant == null) return NotFound();
        return View(enseignant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email")] Enseignant enseignant)
    {
        if (id != enseignant.Id) return NotFound();
        if (!ModelState.IsValid) return View(enseignant);
        try { _context.Update(enseignant); await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { if (!await _context.Enseignants.AnyAsync(e => e.Id == enseignant.Id)) return NotFound(); else throw; }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var enseignant = await _context.Enseignants.FirstOrDefaultAsync(m => m.Id == id.Value);
        if (enseignant == null) return NotFound();
        return View(enseignant);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enseignant = await _context.Enseignants.FindAsync(id);
        if (enseignant != null) { _context.Enseignants.Remove(enseignant); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email")] Enseignant enseignant)
    {
        if (ModelState.IsValid) { _context.Add(enseignant); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(enseignant);
    }
}