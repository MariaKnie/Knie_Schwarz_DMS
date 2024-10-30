using Microsoft.EntityFrameworkCore;
using MyDocDAL.Data;
using MyDocDAL.Repositories;
using Npgsql;
using log4net.Config;
using log4net;

var builder = WebApplication.CreateBuilder(args);

// Configure log4net from the external config file
XmlConfigurator.Configure(new System.IO.FileInfo("log4netDAL.config"));

// Optional: Configure built-in logging (if needed)
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net(); // You may need to add a reference for this



// Add services to the container.
builder.Services.AddControllers();
//
//builder.Services.AddDbContext<MyDocContext>();    //used for migrations
builder.Services.AddDbContext<MyDocContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyDocDatabase")));


builder.Services.AddScoped<ItMyDocRepository, MyDocRepository>();

var app = builder.Build();

// Migrations und Datenbankerstellung anwenden
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyDocContext>();

    // Verbindungstest zur Datenbank
    try
    {
        Console.WriteLine("Versuche, eine Verbindung zur Datenbank herzustellen...");

        //context.Database.EnsureDeleted(); //Use to drop table
        context.Database.Migrate(); //Does function properly ^w^
        // Warte, bis die Datenbank bereit ist
        while (!context.Database.CanConnect())
        {
            Console.WriteLine("Datenbank ist noch nicht bereit, warte...");
            Thread.Sleep(1000); // Warte 1 Sekunde
        }

        Console.WriteLine("Verbindung zur Datenbank erfolgreich.");

        // Migrations anwenden und die Datenbank erstellen/aktualisieren
        context.Database.EnsureCreated();
        Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
    }
}

app.MapControllers();

app.Run();