using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GestionSallesEmit.Data;
using GestionSallesEmit.Hubs;
using GestionSallesEmit.Models;

namespace GestionSallesEmit.Controllers;

[Route("test")]
public class TestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TestController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // GET /test/add-course
    [HttpGet("add-course")]
    public async Task<IActionResult> AddCourse()
    {
        var cours = new Cours { NomCours = "Test Course " + DateTime.Now.ToString("HH:mm:ss"), Credit = 3 };
        _context.Cours.Add(cours);
        await _context.SaveChangesAsync();

        // send SignalR notification (best-effort)
        try { await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"(Test) Nouveau cours ajouté: {cours.NomCours}"); }
        catch { }

        return Content($"Created: {cours.NomCours}");
    }
}
