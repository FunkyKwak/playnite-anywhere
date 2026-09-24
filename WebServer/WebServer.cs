using System;
using System.IO;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Playnite.SDK;
using Playnite.SDK.Models;
using EmbedIOWebServer = EmbedIO.WebServer;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Swan;


namespace PlayniteAnywhere
{
    public class WebServer
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly PreferencesManager preferencesManager;
        private EmbedIOWebServer server;

        public WebServer(IPlayniteAPI playniteApi, PreferencesManager preferencesManager)
        {
            this.playniteApi = playniteApi;
            this.preferencesManager = preferencesManager;
        }

        public void Start(int port)
        {
            server = new EmbedIOWebServer(options =>
                options
                    .WithUrlPrefix($"http://*:{port}/")
                    .WithMode(HttpListenerMode.EmbedIO)
            );

            //API 
            server.WithWebApi("/api", m => m
                .WithController<StatusController>()
                .WithController(() => new GamesController(playniteApi))
                .WithController(() => new PreferencesController(preferencesManager))
            );

            // Serve website files
            var pluginPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location
            );

            var webPath = Path.Combine(pluginPath, "Web");
            server.WithModule(
                new FileModule(
                    "/",
                    new FileSystemProvider(webPath,false)
                )
            );

            server.RunAsync();
        }

        public void Stop()
        {
            if (server == null)
            {
                return;
            }

            server.Dispose();
            server = null;
        }
    }

    public class StatusController : WebApiController
    {
        [Route(HttpVerbs.Get, "/status")]
        public string GetStatus()
        {
            return "Playnite Anywhere fonctionne !";
        }
    }

    public class GamesController : WebApiController
    {
        private readonly IPlayniteAPI playniteApi;

        public GamesController(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
        }

        [Route(HttpVerbs.Get, "/games")]
        public object GetGames()
        {
            return playniteApi.Database.Games.Select(game => new
            {
                id = game.Id,
                name = game.Name,
                favorite = game.Favorite,
                source = game.Source?.Name,
                completionStatus = game.CompletionStatus?.Name,
                cover = string.IsNullOrEmpty(game.CoverImage) ? null : $"/api/covers/{game.Id}"
            });
        }

        [Route(HttpVerbs.Get, "/covers/{id}")]
        public async Task GetCover(Guid id)
        {
            var game = playniteApi.Database.Games[id];

            if (game == null || string.IsNullOrEmpty(game.CoverImage))
            {
                HttpContext.Response.StatusCode = 404;
                return;
            }

            var coverPath = playniteApi.Database.GetFullFilePath(game.CoverImage);

            if (!File.Exists(coverPath))
            {
                HttpContext.Response.StatusCode = 404;
                return;
            }

            var extension = Path.GetExtension(coverPath).ToLowerInvariant();

            string contentType;

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;

                case ".png":
                    contentType = "image/png";
                    break;

                case ".webp":
                    contentType = "image/webp";
                    break;

                case ".gif":
                    contentType = "image/gif";
                    break;

                case ".bmp":
                    contentType = "image/bmp";
                    break;

                default:
                    contentType = "application/octet-stream";
                    break;
            }

            HttpContext.Response.ContentType = contentType;

            using (var input = File.OpenRead(coverPath))
            using (var output = HttpContext.OpenResponseStream(true, false))
            {
                await input.CopyToAsync(output);
            }
        }
    }

    public class PreferencesController : WebApiController
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PreferencesManager preferencesManager;

        public PreferencesController(PreferencesManager preferencesManager)
        {
            this.preferencesManager = preferencesManager;
        }

        [Route(HttpVerbs.Get, "/preferences")]
        public PlayniteAnywherePreferences GetPreferences()
        {
            return preferencesManager.Preferences;
        }

        [Route(HttpVerbs.Post, "/preferences")]
        public void SetPreferences([JsonData] PlayniteAnywherePreferences preferences)
        {
            logger.Info("=== SET PREFERENCES ===");
            logger.Info($"{preferences.ToJson()}");
            logger.Info($"request.Name = {preferences.GroupBy}");

            if (preferences == null)
            {
                HttpContext.Response.StatusCode = 400;
                logger.Error("preferences == null, returns 404");
                return;
            }

            var allowedValues = new[]
            {
                "None",
                "Source",
                "Progress",
                "Platform"
            };

            if (!allowedValues.Contains(preferences.GroupBy))
            {
                HttpContext.Response.StatusCode = 400;
                logger.Error($"GroupBy value '{preferences.GroupBy}' not allowed");
                return;
            }

            preferencesManager.Preferences.GroupBy = preferences.GroupBy;
            preferencesManager.Save();
        }

        [Route(HttpVerbs.Post, "/preferences/groups")]
        public void SetGroupState([JsonData] GroupStateRequest request)
        {
            logger.Info("=== SET GROUP STATE ===");
            logger.Info($"{request.ToJson()}");
            logger.Info($"request.Name = {request.Name}");
            logger.Info($"request.Collapsed = {request.Collapsed}");
            if (request == null)
            {
                HttpContext.Response.StatusCode = 400;
                logger.Error("preferences == null, returns 404");
                return;
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                HttpContext.Response.StatusCode = 400;
                logger.Error($"GroupStateRequest incorrect : '{request.ToJson()}'");
                return;
            }

            preferencesManager.SetGroupCollapsed(
                request.Name,
                request.Collapsed
            );
        }
    }
}