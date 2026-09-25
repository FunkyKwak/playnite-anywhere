var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/status", () => "Playnite Anywhere Backend fonctionne !");

app.Run();
