using System;
using System.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Playnite.SDK;
using Playnite.SDK.Models;
using EmbedIOWebServer = EmbedIO.WebServer;

namespace PlayniteAnywhere
{
    public class WebServer
    {
        private readonly IPlayniteAPI playniteApi;
        private EmbedIOWebServer server;

        public WebServer(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
        }

        public void Start(int port)
        {
            server = new EmbedIOWebServer(options =>
                options
                    .WithUrlPrefix($"http://*:{port}/")
                    .WithMode(HttpListenerMode.EmbedIO)
            );

            server.WithWebApi("/api", m => m
                .WithController<StatusController>()
                .WithController(() => new GamesController(playniteApi))
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
                cover = string.IsNullOrEmpty(game.CoverImage)
                    ? null
                    : $"/covers/{game.Id}"
            });
        }
    }
}