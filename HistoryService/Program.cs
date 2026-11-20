
using HistoryService.Data;
using HistoryService.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("HistoryDatabase");
    options.UseNpgsql(connectionString);

    // Retry connection to CharacterDatbase upon failure:
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
});

// Add database health check;
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

// Build app:
var app = builder.Build();

// Apply any pending migrations before the app starts serving requests
//using (var scope = app.Services.CreateScope())
//{
//    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
//}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.MapPost("/history", async (HistoryEntryRequest req, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Field))
        return Results.BadRequest("Field is required.");

    var entry = new HistoryEntry
    {
        CharacterId = req.CharacterId,
        Field = req.Field,
        OldValue = req.OldValue,
        NewValue = req.NewValue,
        TimeChanged = DateTime.UtcNow,
        Description = string.IsNullOrWhiteSpace(req.Description)
        ? $"{req.Field} changed from '{req.OldValue}' to '{req.NewValue}'"
        : req.Description,
    };
    db.Entries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/history/{entry.CharacterId}", entry);
});

// Endpoint to get all history entries for a specific character
app.MapGet("/history/{characterId:int}", async (int characterId, ApplicationDbContext db) =>
    await db.Entries.Where(e => e.CharacterId == characterId)
                    .OrderByDescending(e => e.TimeChanged)
                    .ToListAsync());

// Endpoint to get a limited amount of history entries for several specified characters
app.MapGet("/history", async (int[] ids, string? field, DateTime from, int? limit, ApplicationDbContext db) =>
{
    // Make sure from is in UTC, if not, convert it to UTC
    var fromUtc = from.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(from, DateTimeKind.Utc)
                            : from.ToUniversalTime();

    var q = db.Entries.Where(e => ids.Contains(e.CharacterId));
    if (!string.IsNullOrEmpty(field))
        q = q.Where(e => e.Field == field)
             .Where(e => e.TimeChanged >= fromUtc);
    return await q.OrderByDescending(e => e.TimeChanged)
                  .Take(Math.Clamp(limit ?? 200, 1, 200)).ToListAsync(); //If no limit is set, set limit to 200. Otherwise, max 200
});

// Add map health checks endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready");

app.Run();