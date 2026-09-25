using Microsoft.EntityFrameworkCore;
using PlayniteAnywhere.Backend.Data;

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

app.MapControllers();

app.MapGet("/api/status", () => "Playnite Anywhere Backend fonctionne !");

app.Run();
