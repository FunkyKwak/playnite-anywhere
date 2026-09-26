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
                FileInfo file = null;
                string coverPath = GetCoverPath(game.Name, game.CoverImage);
                if (coverPath != null)
                    file = new FileInfo(coverPath);

                games.Add(new SyncGameRequest
                {
                    Id = game.Id,
                    Name = game.Name,
                    Favorite = game.Favorite,
                    Source = game.Source?.Name,
                    CompletionStatus = game.CompletionStatus?.Name,
                    CoverSize = file?.Length,
                    CoverLastWriteTimeUtcTicks = file?.LastWriteTimeUtc.Ticks
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

            SyncGamesResponse syncResult;
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

                var responseBody = await response.Content.ReadAsStringAsync();

                serializer = new DataContractJsonSerializer(
                    typeof(SyncGamesResponse)
                );


                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody)))
                {
                    syncResult = (SyncGamesResponse)serializer.ReadObject(stream);
                }

                logger.Info(
                    $"Synchronisation terminée : " +
                    $"{syncResult.received} jeux reçus, " +
                    $"{syncResult.deleted} supprimés, " +
                    $"{syncResult.coversToSync.Count} covers à synchroniser."
                );
            }


            foreach (var game in playniteApi.Database.Games)
            {
                if (!syncResult.coversToSync.Contains(game.Id))
                {
                    continue;
                }

                var coverPath = GetCoverPath(game.Name, game.CoverImage);
                if (coverPath != null)
                {
                    await SyncCover(
                        game.Id,
                        coverPath
                    );
                }
            }
            logger.Info($"Synchronisation des covers terminée");
        }

        private string GetCoverPath(string gameName, string coverImage)
        {
            if (string.IsNullOrEmpty(coverImage))
            {
                return null;
            }

            var coverPath = playniteApi.Database.GetFullFilePath(coverImage);

            if (string.IsNullOrEmpty(coverPath) || !File.Exists(coverPath))
            {
                logger.Warn($"Cover introuvable pour {gameName} : {coverPath}");
                return null;
            }
            
            return coverPath;
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