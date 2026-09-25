using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK;
using PlayniteAnywhere.Common.Models;

namespace PlayniteAnywhere
{
    public class SyncManager
    {
        private static readonly ILogger logger =
            LogManager.GetLogger();

        private readonly IPlayniteAPI playniteApi;
        private readonly string serverUrl;

        private readonly HttpClient httpClient;

        public SyncManager(
            IPlayniteAPI playniteApi,
            string serverUrl)
        {
            this.playniteApi = playniteApi;
            this.serverUrl = serverUrl.TrimEnd('/');

            httpClient = new HttpClient();
        }

        public async Task SyncGames()
        {
            logger.Info("=== PLAYNITE ANYWHERE : SYNCHRONISATION ===");

            var games = new List<SyncGameRequest>();

            foreach (var game in playniteApi.Database.Games)
            {
                games.Add(new SyncGameRequest
                {
                    Id = game.Id,
                    Name = game.Name,
                    Favorite = game.Favorite,
                    Source = game.Source?.Name,
                    CompletionStatus = game.CompletionStatus?.Name
                });
            }

            logger.Info(
                $"Synchronisation de {games.Count} jeux..."
            );

            var json = JsonConvert.SerializeObject(games);

            using (var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"))
            {
                var response = await httpClient.PostAsync(
                    $"{serverUrl}/api/sync/games",
                    content
                );

                response.EnsureSuccessStatusCode();

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                logger.Info(
                    $"Synchronisation terminée : {responseBody}"
                );
            }
        }
    }
}