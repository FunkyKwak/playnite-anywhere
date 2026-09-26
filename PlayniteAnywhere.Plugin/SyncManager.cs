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

            

            foreach (var game in playniteApi.Database.Games)
            {
                if (!string.IsNullOrEmpty(game.CoverImage))
                {
                    var coverPath = playniteApi.Database.GetFullFilePath(game.CoverImage);

                    if (string.IsNullOrEmpty(coverPath) || !File.Exists(coverPath))
                    {
                        logger.Warn($"Cover introuvable pour {game.Name} : {coverPath}");
                        continue;
                    }

                    await SyncCover(
                        game.Id,
                        coverPath
                    );
                }
            }
            logger.Info($"Synchronisation des covers terminée");
        }

        private async Task SyncCover(Guid gameId, string coverPath)
        {
            logger.Info($"Synchronisation de la cover : {gameId}");

            using (var content = new MultipartFormDataContent())
            using (var fileStream = File.OpenRead(coverPath))
            using (var fileContent = new StreamContent(fileStream))
            {
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        GetContentType(Path.GetExtension(coverPath))
                    );

                content.Add(
                    fileContent,
                    "file",
                    Path.GetFileName(coverPath)
                );

                var response = await httpClient.PostAsync(
                    $"{serverUrl}/api/sync/covers/{gameId}",
                    content
                );

                response.EnsureSuccessStatusCode();
            }
        }

        private static string GetContentType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";

                case ".png":
                    return "image/png";

                case ".webp":
                    return "image/webp";

                case ".gif":
                    return "image/gif";

                case ".bmp":
                    return "image/bmp";

                default:
                    return "application/octet-stream";
            }
        }
    }
}