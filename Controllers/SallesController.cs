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

    // GET: Salles (pour l'UI dynamique)
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ListJson(string q = "", string sort = "name", int page = 1, int pageSize = 10, int minCapacite = 0)
    {
        var query = _context.Salles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(s => s.NomSalle.Contains(q));
        if (minCapacite > 0) query = query.Where(s => s.Capacite >= minCapacite);
        var total = await query.CountAsync();
        query = sort switch { "cap" => query.OrderByDescending(s => s.Capacite), _ => query.OrderBy(s => s.NomSalle) };
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        // Determine occupancy for each room based on today's seances
        var ci = new System.Globalization.CultureInfo("fr-FR");
        var todayName = ci.TextInfo.ToTitleCase(DateTime.Now.ToString("dddd", ci));
        var now = DateTime.Now.TimeOfDay;
        var seancesToday = await _context.Seances.Where(se => se.Jour == todayName).ToListAsync();
        var seancesBySalle = seancesToday.GroupBy(se => se.SalleId).ToDictionary(g => g.Key, g => g.ToList());

        var resultItems = items.Select(s =>
        {
            bool occupied = false;
            string? occupiedUntil = null;
            if (seancesBySalle.TryGetValue(s.Id, out var list))
            {
                foreach (var se in list)
                {
                    if (TimeSpan.TryParse(se.HeureDebut, out var sd) && TimeSpan.TryParse(se.HeureFin, out var ed))
                    {
                        if (sd <= now && now < ed)
                        {
                            occupied = true;
                            occupiedUntil = ed.ToString("hh\\:mm");
                            break;
                        }
                    }
                }
            }
            return new { id = s.Id, nomSalle = s.NomSalle, capacite = s.Capacite, isOccupied = occupied, occupiedUntil };
        }).ToList();

        return Json(new { items = resultItems, total });
    }

    [HttpGet]
    public async Task<IActionResult> SummaryJson(int minCapacite = 0)
    {
        var query = _context.Salles.AsQueryable();
        if (minCapacite > 0) query = query.Where(s => s.Capacite >= minCapacite);

        var total = await query.CountAsync();
        var totalCapacity = await query.SumAsync(s => s.Capacite);
        var avgCapacity = await query.AnyAsync() ? await query.AverageAsync(s => s.Capacite) : 0;
        var topRoom = await query.OrderByDescending(s => s.Capacite).FirstOrDefaultAsync();

        // compute occupied now
        var ci = new System.Globalization.CultureInfo("fr-FR");
        var todayName = ci.TextInfo.ToTitleCase(DateTime.Now.ToString("dddd", ci));
        var now = DateTime.Now.TimeOfDay;
        var seancesToday = await _context.Seances.Where(se => se.Jour == todayName).ToListAsync();
        var occupiedNow = seancesToday.Where(se => TimeSpan.TryParse(se.HeureDebut, out var sd) && TimeSpan.TryParse(se.HeureFin, out var ed) && sd <= now && now < ed).Select(s => s.SalleId).Distinct().Count();

        return Json(new
        {
            total,
            totalCapacity,
            avgCapacity = Math.Round(avgCapacity, 1),
            largestRoom = topRoom == null ? null : new { nomSalle = topRoom.NomSalle, capacite = topRoom.Capacite },
            occupiedNow
        });
    }

    // Edit
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var salle = await _context.Salles.FindAsync(id.Value);
        if (salle == null) return NotFound();
        return View(salle);
    }

    // Delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var salle = await _context.Salles.FirstOrDefaultAsync(m => m.Id == id.Value);
        if (salle == null) return NotFound();
        return View(salle);
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
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_Form", salle);
        return View(salle);
    }

    [HttpGet]
    public IActionResult CreatePartial()
    {
        return PartialView("_Form", new Salle());
    }

    [HttpGet]
    public async Task<IActionResult> EditPartial(int? id)
    {
        if (id == null) return NotFound();
        var salle = await _context.Salles.FindAsync(id.Value);
        if (salle == null) return NotFound();
        return PartialView("_Form", salle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomSalle,Capacite")] Salle salle)
    {
        if (id != salle.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Form", salle);
            return View(salle);
        }
        try { _context.Update(salle); await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Salles.AnyAsync(e => e.Id == salle.Id)) return NotFound();
            throw;
        }
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true });
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DeletePartial(int? id)
    {
        if (id == null) return NotFound();
        var salle = await _context.Salles.FirstOrDefaultAsync(m => m.Id == id.Value);
        if (salle == null) return NotFound();
        return PartialView("_DeleteConfirmation", salle);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var salle = await _context.Salles.FindAsync(id);
        if (salle != null)
        {
            _context.Salles.Remove(salle);
            await _context.SaveChangesAsync();
        }
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true });
        return RedirectToAction(nameof(Index));
    }
}