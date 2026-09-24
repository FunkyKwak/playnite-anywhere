using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using EmbedIOWebServer = EmbedIO.WebServer;

namespace PlayniteAnywhere
{
    public class WebServer
    {
        private EmbedIOWebServer server;

        public void Start(int port)
        {
            server = new EmbedIOWebServer(options =>
                options
                    .WithUrlPrefix($"http://*:{port}/")
                    .WithMode(HttpListenerMode.EmbedIO)
            );

            server.WithWebApi("/api", m => m
                .WithController<StatusController>()
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
}