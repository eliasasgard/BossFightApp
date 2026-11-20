using CharacterWebsite.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<CharacterClient>(client => 
{
    var baseUrl = builder.Configuration["CharacterService:BaseUrl"]
        ?? throw new InvalidOperationException("CharacterService:BaseUrl not working"); 
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//app.UseAuthorization();

//app.MapStaticAssets();
app.MapDefaultControllerRoute();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
