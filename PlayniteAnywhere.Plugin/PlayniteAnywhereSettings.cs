using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteAnywhere
{
    public class PlayniteAnywhereSettings : ObservableObject
    {
        private bool useLocalWebServer = true;
        public bool UseLocalWebServer
        {
            get => useLocalWebServer;
            set => SetValue(ref useLocalWebServer, value);
        }
        private string syncServerUrl = string.Empty;
        public string SyncServerUrl { get => syncServerUrl; set => SetValue(ref syncServerUrl, value); }
    }

    public class PlayniteAnywhereSettingsViewModel : ObservableObject, ISettings
    {
        private readonly PlayniteAnywhere plugin;
        private PlayniteAnywhereSettings editingClone { get; set; }
        public string LocalUrl { get; }
        
        public bool UseLocalWebServer
        {
            get => Settings.UseLocalWebServer;
            set
            {
                Settings.UseLocalWebServer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLocalWebServer));
                OnPropertyChanged(nameof(IsRemoteWebServer));
            }
        }
        public bool IsLocalWebServer => UseLocalWebServer;
        public bool IsRemoteWebServer => !UseLocalWebServer;


        private string originalSyncServerUrl;
        private bool originalUseLocalWebServer;

        private PlayniteAnywhereSettings settings;
        public PlayniteAnywhereSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public PlayniteAnywhereSettingsViewModel(PlayniteAnywhere plugin)
        {
            LocalUrl = $"http://{GetLocalIpAddress()}:32650";

            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<PlayniteAnywhereSettings>();
            
            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new PlayniteAnywhereSettings();
            }
            UseLocalWebServer = Settings.UseLocalWebServer;
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);

            originalSyncServerUrl = Settings.SyncServerUrl;
            originalUseLocalWebServer = UseLocalWebServer;
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            var serverSettingsChanged =
                originalUseLocalWebServer != UseLocalWebServer ||
                originalSyncServerUrl != Settings.SyncServerUrl;
                
            plugin.SavePluginSettings(Settings);

            if (serverSettingsChanged)
            {
                plugin.PlayniteApi.Dialogs.ShowMessage(
                    "Les paramètres du serveur Web ont été modifiés.\n\n" +
                    "Vous devez redémarrer Playnite pour appliquer ces changements."
                );
            }
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            if (!Settings.UseLocalWebServer && string.IsNullOrWhiteSpace(Settings.SyncServerUrl))
            {
                errors.Add("Dans ce mode, vous devez préciser l'URL du serveur distant.");
            }
            return errors.Count == 0;
        }

        private static string GetLocalIpAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i =>
                    i.OperationalStatus == OperationalStatus.Up &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

            foreach (var networkInterface in interfaces)
            {
                var address = networkInterface.GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(a =>
                        a.Address.AddressFamily == AddressFamily.InterNetwork);

                if (address != null)
                {
                    return address.Address.ToString();
                }
            }

            return "Adresse IP introuvable";
        }
    }
}