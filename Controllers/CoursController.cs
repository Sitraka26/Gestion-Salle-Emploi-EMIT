using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;
using GestionSallesEmit.Hubs;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

public class CoursController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public CoursController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private static readonly string[] Niveaux = { "L1", "L2", "L3", "M1", "M2" };

    public IActionResult Index() => View();

    public IActionResult Create()
    {
        ViewBag.Parcours = new SelectList(_context.Parcours, "Id", "NomParcours");
        ViewBag.Niveaux = new SelectList(Niveaux);
        return View();
    }

    [HttpGet]
    public IActionResult CreatePartial()
    {
        ViewBag.Parcours = new SelectList(_context.Parcours, "Id", "NomParcours");
        ViewBag.Niveaux = new SelectList(Niveaux);
        return PartialView("_Form", new Cours());
    }

    [HttpGet]
    public async Task<IActionResult> ListJson(string q = "", string sort = "name", int page = 1, int pageSize = 10, int minCredit = 0)
    {
        var query = _context.Cours.Include(c => c.Parcours).ThenInclude(p => p!.Mention).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.NomCours.Contains(q));
        if (minCredit > 0)
            query = query.Where(c => c.Credit >= minCredit);

        var total = await query.CountAsync();

        query = sort switch
        {
            "credit" => query.OrderByDescending(c => c.Credit),
            _ => query.OrderBy(c => c.NomCours),
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new {
                c.Id,
                c.NomCours,
                c.Credit,
                c.Niveau,
                parcours = c.Parcours != null ? c.Parcours.NomParcours : null,
                mention = c.Parcours != null && c.Parcours.Mention != null ? c.Parcours.Mention.NomMention : null
            })
            .ToListAsync();

        return Json(new { items, total });
    }

    [HttpGet]
    public async Task<IActionResult> SummaryJson(int minCredit = 0)
    {
        var query = _context.Cours.AsQueryable();
        if (minCredit > 0)
            query = query.Where(c => c.Credit >= minCredit);

        var total = await query.CountAsync();
        var avgCredit = await query.AnyAsync() ? await query.AverageAsync(c => c.Credit) : 0;
        var top = await query.OrderByDescending(c => c.Credit).FirstOrDefaultAsync();

        return Json(new
        {
            total,
            avgCredit = Math.Round(avgCredit, 1),
            topCourse = top == null ? null : new { nomCours = top.NomCours, credit = top.Credit }
        });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var cours = await _context.Cours.FindAsync(id.Value);
        if (cours == null) return NotFound();
        ViewBag.Parcours = new SelectList(_context.Parcours, "Id", "NomParcours", cours.ParcoursId);
        ViewBag.Niveaux = new SelectList(Niveaux, cours.Niveau);
        return View(cours);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomCours,Credit,Niveau,ParcoursId")] Cours cours)
    {
        if (id != cours.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Parcours = new SelectList(_context.Parcours, "Id", "NomParcours", cours.ParcoursId);
            ViewBag.Niveaux = new SelectList(Niveaux, cours.Niveau);
            return View(cours);
        }

        try
        {
            _context.Update(cours);
            await _context.SaveChangesAsync();

            try { await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Cours modifié: {cours.NomCours}"); } catch { }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cours.AnyAsync(e => e.Id == cours.Id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var cours = await _context.Cours.Include(c => c.Parcours).FirstOrDefaultAsync(m => m.Id == id.Value);
        if (cours == null) return NotFound();
        return View(cours);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cours = await _context.Cours.FindAsync(id);
        if (cours != null)
        {
            _context.Cours.Remove(cours);
            await _context.SaveChangesAsync();
            try { await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Cours supprimé: {cours.NomCours}"); } catch { }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NomCours,Credit,Niveau,ParcoursId")] Cours cours)
    {
        if (ModelState.IsValid)
        {
            _context.Add(cours);
            await _context.SaveChangesAsync();

            try { await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Nouveau cours ajouté: {cours.NomCours}"); } catch { }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Parcours = new SelectList(_context.Parcours, "Id", "NomParcours", cours.ParcoursId);
        ViewBag.Niveaux = new SelectList(Niveaux, cours.Niveau);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return PartialView("_Form", cours);
        return View(cours);
    }
}