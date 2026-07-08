using Microsoft.EntityFrameworkCore;
using GestionSallesEmit.Data;

namespace GestionSallesEmit;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuration de la connexion PostgreSQL
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSignalR();

        var app = builder.Build();

        // Seed initial data for Salles, Enseignants and Cours (useful for demo)
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                SeedData.EnsureSeedData(ctx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seed error: {ex.Message}");
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapHub<GestionSallesEmit.Hubs.NotificationHub>("/hubs/notifications");

        app.Run();
    }
}