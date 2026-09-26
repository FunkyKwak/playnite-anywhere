using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<PlayniteAnywhereDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
    )
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<PlayniteAnywhereDbContext>();

    db.Database.Migrate();
}


var webPath = Path.Combine(
    AppContext.BaseDirectory,
    "web"
);

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(webPath)
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webPath)
});



app.MapControllers();

app.MapGet("/api/status", () => "Playnite Anywhere Backend fonctionne !");

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
