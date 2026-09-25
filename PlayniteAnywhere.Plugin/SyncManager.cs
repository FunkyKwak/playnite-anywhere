using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using System.Runtime.Serialization.Json;
using PlayniteAnywhere.Common.Models;
using System.IO;

namespace PlayniteAnywhere
{
    public class SyncManager
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly IPlayniteAPI playniteApi;
        private readonly string serverUrl;

        private readonly HttpClient httpClient;

        public SyncManager(IPlayniteAPI playniteApi, string serverUrl)
        {
            this.playniteApi = playniteApi;
            if(!string.IsNullOrWhiteSpace(serverUrl))
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
                $"Synchronisation de {games.Count} jeux vers le serveur {serverUrl}..."
            );

            string json;

            var serializer = new DataContractJsonSerializer(
                typeof(List<SyncGameRequest>)
            );

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, games);

                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
            }

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