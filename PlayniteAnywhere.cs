using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PlayniteAnywhere
{
    public class PlayniteAnywhere : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private PlayniteAnywhereSettingsViewModel settings { get; set; }
        public override Guid Id { get; } = Guid.Parse("1b4b762f-1a9a-423b-9643-1f323ef5ef71");

        private WebServer webServer;
        private readonly IPlayniteAPI playniteApi;
        private PreferencesManager preferencesManager;

        public PlayniteAnywhere(IPlayniteAPI api) : base(api)
        {
            playniteApi = api;
            settings = new PlayniteAnywhereSettingsViewModel(this);
            preferencesManager = new PreferencesManager(Path.Combine(playniteApi.Paths.ExtensionsDataPath, this.Id.ToString()));
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
            try
            {
                logger.Info("Playnite Anywhere started.");
    
                webServer = new WebServer(playniteApi, preferencesManager);
                webServer.Start(32650);

                logger.Info("Playnite Anywhere web server started on port 32650.");
            }
            catch(Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
            webServer.Stop();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayniteAnywhereSettingsView();
        }
    }
}