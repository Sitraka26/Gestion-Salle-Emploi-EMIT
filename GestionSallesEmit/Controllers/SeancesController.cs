using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using GestionSallesEmit.Data;
using GestionSallesEmit.Models;
using GestionSallesEmit.Hubs;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;


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

    public async Task<IActionResult> Index()
    {
        var seances = await _context.Seances
            .Include(s => s.Salle)
            .Include(s => s.Enseignant)
            .Include(s => s.Cours)
            .ToListAsync();

        ViewBag.Enseignants = new SelectList(await _context.Enseignants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToListAsync(), "Id", "Nom");
        ViewBag.Salles = new SelectList(await _context.Salles.OrderBy(s => s.NomSalle).ToListAsync(), "Id", "NomSalle");

        return View(seances);
    }

    [HttpGet]
    public async Task<IActionResult> ListJson(string q = "", string jour = "", int? enseignantId = null, int? salleId = null, int minCapacity = 0, bool availableNow = false, int page = 1, int pageSize = 10)
    {
        var query = _context.Seances.Include(s => s.Salle).Include(s => s.Enseignant).Include(s => s.Cours).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(s => s.Cours!.NomCours.Contains(q) || s.Enseignant!.Nom.Contains(q) || s.Salle!.NomSalle.Contains(q));
        if (!string.IsNullOrWhiteSpace(jour)) query = query.Where(s => s.Jour == jour);
        if (enseignantId.HasValue) query = query.Where(s => s.EnseignantId == enseignantId.Value);
        if (salleId.HasValue) query = query.Where(s => s.SalleId == salleId.Value);
        if (minCapacity > 0) query = query.Where(s => s.Salle!.Capacite >= minCapacity);

        var seances = await query.ToListAsync();
        if (availableNow)
        {
            var ci = new System.Globalization.CultureInfo("fr-FR");
            var todayName = ci.TextInfo.ToTitleCase(DateTime.Now.ToString("dddd", ci));
            var now = DateTime.Now.TimeOfDay;
            seances = seances.Where(s => s.Jour == todayName &&
                TimeSpan.TryParse(s.HeureDebut, out var sd) &&
                TimeSpan.TryParse(s.HeureFin, out var ed) &&
                sd <= now && now < ed).ToList();
        }

        var total = seances.Count;
        var items = seances.OrderBy(s => s.Jour).ThenBy(s => s.HeureDebut).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new { s.Id, jour = s.Jour, heureDebut = s.HeureDebut, heureFin = s.HeureFin, cours = s.Cours!.NomCours, enseignant = s.Enseignant!.Nom + " " + s.Enseignant!.Prenom, salle = s.Salle!.NomSalle }).ToList();
        return Json(new { items, total });
    }

    [HttpGet]
    public async Task<IActionResult> SummaryJson()
    {
        var total = await _context.Seances.CountAsync();
        var busiestDay = await _context.Seances
            .GroupBy(s => s.Jour)
            .Select(g => new { jour = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count)
            .FirstOrDefaultAsync();

        var topTeacher = await _context.Seances
            .GroupBy(s => s.EnseignantId)
            .Select(g => new { id = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count)
            .FirstOrDefaultAsync();

        var topTeacherName = topTeacher == null ? null : await _context.Enseignants
            .Where(e => e.Id == topTeacher.id)
            .Select(e => e.Nom + " " + e.Prenom)
            .FirstOrDefaultAsync();

        var topRoom = await _context.Seances
            .GroupBy(s => s.SalleId)
            .Select(g => new { id = g.Key, count = g.Count() })
            .OrderByDescending(g => g.count)
            .FirstOrDefaultAsync();

        var topRoomName = topRoom == null ? null : await _context.Salles
            .Where(s => s.Id == topRoom.id)
            .Select(s => s.NomSalle)
            .FirstOrDefaultAsync();

        return Json(new
        {
            total,
            busiestDay = busiestDay == null ? null : new { busiestDay.jour, busiestDay.count },
            topTeacher = topTeacherName,
            topRoom = topRoomName
        });
    }

    [HttpGet]
    public async Task<IActionResult> Events(string q = "", string jour = "", int? enseignantId = null, int? salleId = null, int minCapacity = 0, bool availableNow = false)
    {
        var query = _context.Seances.Include(s => s.Salle).Include(s => s.Enseignant).Include(s => s.Cours).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(s => s.Cours!.NomCours.Contains(q) || s.Enseignant!.Nom.Contains(q) || s.Salle!.NomSalle.Contains(q));
        if (!string.IsNullOrWhiteSpace(jour)) query = query.Where(s => s.Jour == jour);
        if (enseignantId.HasValue) query = query.Where(s => s.EnseignantId == enseignantId.Value);
        if (salleId.HasValue) query = query.Where(s => s.SalleId == salleId.Value);
        if (minCapacity > 0) query = query.Where(s => s.Salle!.Capacite >= minCapacity);

        var seances = await query.ToListAsync();
        if (availableNow)
        {
            var ci = new System.Globalization.CultureInfo("fr-FR");
            var todayName = ci.TextInfo.ToTitleCase(DateTime.Now.ToString("dddd", ci));
            var now = DateTime.Now.TimeOfDay;
            seances = seances.Where(s => s.Jour == todayName &&
                TimeSpan.TryParse(s.HeureDebut, out var sd) &&
                TimeSpan.TryParse(s.HeureFin, out var ed) &&
                sd <= now && now < ed).ToList();
        }

        var today = DateTime.Today;
        int diff = (7 + (int)today.DayOfWeek - 1) % 7;
        var monday = today.AddDays(-diff);

        var events = seances.Select(s => {
            var dayNum = DayToNumber(s.Jour);
            int offset = dayNum == 0 ? 6 : (dayNum - 1);
            var date = monday.AddDays(offset);
            DateTime startDt, endDt;
            if (!DateTime.TryParseExact(s.HeureDebut, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var sd))
                sd = DateTime.Today.AddHours(8);
            if (!DateTime.TryParseExact(s.HeureFin, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var ed))
                ed = sd.AddHours(1);
            startDt = new DateTime(date.Year, date.Month, date.Day, sd.Hour, sd.Minute, 0);
            endDt = new DateTime(date.Year, date.Month, date.Day, ed.Hour, ed.Minute, 0);

            return new {
                id = s.Id,
                title = $"{s.Cours?.NomCours} - {s.Enseignant?.Nom} ({s.Salle?.NomSalle})",
                start = startDt.ToString("s"),
                end = endDt.ToString("s"),
                backgroundColor = "#0d6efd",
                extendedProps = new { salleId = s.SalleId }
            };
        });

        return Json(events);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventDto dto)
    {
        if (dto == null || dto.Id <= 0) return BadRequest();
        var seance = await _context.Seances.FindAsync(dto.Id);
        if (seance == null) return NotFound();

        if (!DateTime.TryParse(dto.Start, out var startDt) || !DateTime.TryParse(dto.End, out var endDt))
            return BadRequest("Invalid dates");

        seance.Jour = startDt.ToString("dddd", System.Globalization.CultureInfo.CurrentCulture).ToString();
        seance.HeureDebut = startDt.ToString("HH:mm");
        seance.HeureFin = endDt.ToString("HH:mm");

        _context.Update(seance);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("SeanceUpdated", new {
            id = seance.Id,
            jour = seance.Jour,
            heureDebut = seance.HeureDebut,
            heureFin = seance.HeureFin,
            salleId = seance.SalleId,
            enseignantId = seance.EnseignantId,
            coursId = seance.CoursId
        });

        return Json(new { success = true });
    }

    public class UpdateEventDto { public int Id { get; set; } public string Start { get; set; } = ""; public string End { get; set; } = ""; }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Seance seance)
    {
        var candidats = await _context.Seances
            .Where(s => s.Jour == seance.Jour && (s.SalleId == seance.SalleId || s.EnseignantId == seance.EnseignantId))
            .ToListAsync();
        var conflit = candidats.Any(s => CreneauxSeChevauchent(s.HeureDebut, s.HeureFin, seance.HeureDebut, seance.HeureFin));

        var indisponibilites = await _context.Indisponibilites
            .Where(i => i.EnseignantId == seance.EnseignantId && i.Jour == seance.Jour)
            .ToListAsync();
        var indisponible = indisponibilites.Any(i => CreneauxSeChevauchent(i.HeureDebut, i.HeureFin, seance.HeureDebut, seance.HeureFin));

        if (conflit || indisponible)
        {
            if (conflit)
                ModelState.AddModelError("", "🚨 Alerte Conflit : La salle ou l'enseignant est déjà affecté à un cours sur ce créneau !");
            if (indisponible)
                ModelState.AddModelError("", "🚫 Cet enseignant a déclaré être indisponible sur ce créneau (pris par une autre université).");

            ViewBag.Salles = new SelectList(_context.Salles, "Id", "NomSalle", seance.SalleId);
            ViewBag.Enseignants = new SelectList(_context.Enseignants, "Id", "Nom", seance.EnseignantId);
            ViewBag.Cours = new SelectList(_context.Cours, "Id", "NomCours", seance.CoursId);
            return View(seance);
        }

        _context.Add(seance);
        await _context.SaveChangesAsync();

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

    // Génère l'emploi du temps complet en PDF
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var seances = await _context.Seances
            .Include(s => s.Salle).Include(s => s.Enseignant).Include(s => s.Cours)
            .ToListAsync();

        var joursOrdre = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
        var seancesTriees = seances
            .OrderBy(s => Array.IndexOf(joursOrdre, s.Jour))
            .ThenBy(s => s.HeureDebut)
            .ToList();

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Emploi du Temps - EMIT").FontSize(20).Bold().FontColor("#0A2E52");
                    col.Item().Text($"Généré le {DateTime.Now:dd/MM/yyyy à HH:mm}").FontSize(9).FontColor("#5B6B7A");
                });

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.4f);
                    });

                    table.Header(header =>
                    {
                        string[] titres = { "Jour", "Horaire", "Cours", "Enseignant", "Salle" };
                        foreach (var titre in titres)
                        {
                            header.Cell().Background("#0A2E52").Padding(6).Text(titre).FontColor("#fff").Bold();
                        }
                    });

                    foreach (var s in seancesTriees)
                    {
                        table.Cell().Padding(6).BorderBottom(1).BorderColor("#DDDDDD").Text(s.Jour);
                        table.Cell().Padding(6).BorderBottom(1).BorderColor("#DDDDDD").Text($"{s.HeureDebut} - {s.HeureFin}");
                        table.Cell().Padding(6).BorderBottom(1).BorderColor("#DDDDDD").Text(s.Cours?.NomCours ?? "-");
                        table.Cell().Padding(6).BorderBottom(1).BorderColor("#DDDDDD").Text($"{s.Enseignant?.Nom} {s.Enseignant?.Prenom}");
                        table.Cell().Padding(6).BorderBottom(1).BorderColor("#DDDDDD").Text(s.Salle?.NomSalle ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text("GestionSallesEmit").FontSize(8).FontColor("#5B6B7A");
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", "EmploiDuTemps_EMIT.pdf");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var seance = await _context.Seances.FindAsync(id);
        if (seance == null) return NotFound();

        ViewBag.Salles = new SelectList(_context.Salles, "Id", "NomSalle", seance.SalleId);
        ViewBag.Enseignants = new SelectList(_context.Enseignants, "Id", "Nom", seance.EnseignantId);
        ViewBag.Cours = new SelectList(_context.Cours, "Id", "NomCours", seance.CoursId);
        return View(seance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Seance seance)
    {
        if (id != seance.Id) return NotFound();

        var candidats = await _context.Seances
            .Where(s => s.Id != seance.Id && s.Jour == seance.Jour && (s.SalleId == seance.SalleId || s.EnseignantId == seance.EnseignantId))
            .ToListAsync();
        var conflit = candidats.Any(s => CreneauxSeChevauchent(s.HeureDebut, s.HeureFin, seance.HeureDebut, seance.HeureFin));

        var indisponibilites = await _context.Indisponibilites
            .Where(i => i.EnseignantId == seance.EnseignantId && i.Jour == seance.Jour)
            .ToListAsync();
        var indisponible = indisponibilites.Any(i => CreneauxSeChevauchent(i.HeureDebut, i.HeureFin, seance.HeureDebut, seance.HeureFin));

        if (conflit)
        {
            ModelState.AddModelError("", "🚨 Alerte Conflit : la salle ou l'enseignant est déjà pris sur ce créneau !");
        }
        if (indisponible)
        {
            ModelState.AddModelError("", "🚫 Cet enseignant a déclaré être indisponible sur ce créneau (pris par une autre université).");
        }

        if (ModelState.IsValid && !conflit && !indisponible)
        {
            _context.Update(seance);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SeanceUpdated", new { id = seance.Id, jour = seance.Jour, heureDebut = seance.HeureDebut, heureFin = seance.HeureFin });
            TempData["SuccessMessage"] = "Séance modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Salles = new SelectList(_context.Salles, "Id", "NomSalle", seance.SalleId);
        ViewBag.Enseignants = new SelectList(_context.Enseignants, "Id", "Nom", seance.EnseignantId);
        ViewBag.Cours = new SelectList(_context.Cours, "Id", "NomCours", seance.CoursId);
        return View(seance);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var seance = await _context.Seances
            .Include(s => s.Salle).Include(s => s.Enseignant).Include(s => s.Cours)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (seance == null) return NotFound();
        return View(seance);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var seance = await _context.Seances.FindAsync(id);
        if (seance != null)
        {
            _context.Seances.Remove(seance);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SeanceDeleted", new { id });
            TempData["SuccessMessage"] = "Séance supprimée.";
        }
        return RedirectToAction(nameof(Index));
    }
    private bool CreneauxSeChevauchent(string debut1, string fin1, string debut2, string fin2)
    {
        if (TimeSpan.TryParse(debut1, out var d1) && TimeSpan.TryParse(fin1, out var f1) &&
            TimeSpan.TryParse(debut2, out var d2) && TimeSpan.TryParse(fin2, out var f2))
        {
            return d1 < f2 && d2 < f1;
        }
        return debut1 == debut2;
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
public async Task<IActionResult> ParClasse(int? parcoursId, string? niveau)
    {
        ViewBag.ParcoursListe = new SelectList(
            await _context.Parcours.Include(p => p.Mention).OrderBy(p => p.NomParcours).ToListAsync(),
            "Id", "NomParcours", parcoursId);
        ViewBag.NiveauxListe = new SelectList(new[] { "L1", "L2", "L3", "M1", "M2" }, niveau);
        ViewBag.ParcoursId = parcoursId;
        ViewBag.Niveau = niveau;

        var model = new PlanningClasseViewModel();
        foreach (var j in model.Jours) model.BlocsParJour[j] = new List<BlocSeance>();

        if (parcoursId.HasValue && !string.IsNullOrEmpty(niveau))
        {
            var parcours = await _context.Parcours.Include(p => p.Mention)
                .FirstOrDefaultAsync(p => p.Id == parcoursId.Value);

            if (parcours != null)
            {
                model.MentionNom = parcours.Mention?.NomMention ?? "";
                model.ParcoursNom = parcours.NomParcours;
                model.Niveau = niveau;

                var seances = await _context.Seances
                    .Include(s => s.Cours).Include(s => s.Enseignant).Include(s => s.Salle)
                    .Where(s => s.Cours!.ParcoursId == parcoursId.Value && s.Cours!.Niveau == niveau)
                    .ToListAsync();

                string[] palette = { "#5fb3e8", "#5cbf8e", "#f2a35c", "#c98ee0", "#e07a7a", "#8ecfe0", "#e8d15f", "#a3d977", "#f28cb1" };

                foreach (var s in seances)
                {
                    if (!model.BlocsParJour.ContainsKey(s.Jour)) continue;
                    if (!TimeSpan.TryParse(s.HeureDebut, out var debut) || !TimeSpan.TryParse(s.HeureFin, out var fin)) continue;

                    int heureDebut = debut.Hours;
                    int dureeMinutes = (int)(fin - debut).TotalMinutes;
                    int duree = Math.Max(1, (int)Math.Ceiling(dureeMinutes / 60.0));

                    int idxCouleur = Math.Abs(s.Cours!.NomCours.GetHashCode()) % palette.Length;

                    model.BlocsParJour[s.Jour].Add(new BlocSeance
                    {
                        HeureDebut = heureDebut,
                        DureeHeures = duree,
                        Cours = s.Cours!.NomCours,
                        Enseignant = $"{s.Enseignant?.Nom} {s.Enseignant?.Prenom}",
                        Salle = s.Salle?.NomSalle ?? "-",
                        Couleur = palette[idxCouleur]
                    });
                }
            }
        }

        return View(model);
    }