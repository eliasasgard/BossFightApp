//Elias Åsgård, 2026-10-29
using CharacterService.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CharacterDatabase");
    options.UseNpgsql(connectionString);

    //Retry connection to CharacterDatbase upon failure:
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
});

//builder.Services.AddHttpClient<HistoryClient>(c =>
//    c.BaseAddress = new Uri(builder.Configuration["HistoryService:BaseUrl"]!));

builder.Services.AddHttpClient<HistoryClient>(client =>
{
    var baseUrl = builder.Configuration["HistoryService:BaseUrl"]
        ?? throw new InvalidOperationException("HistoryService:BaseUrl missing");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddProblemDetails();

// Add database health check;
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler();

app.MapControllers();

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

// Add map health checks endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready");

app.Run();
