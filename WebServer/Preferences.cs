using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Swan.Logging;

namespace PlayniteAnywhere
{
    [DataContract]
    public class PlayniteAnywherePreferences
    {
        [DataMember]
        public string GroupBy { get; set; } = "None";
    }

    public class PreferencesManager
    {
        private readonly string preferencesPath;

        public PlayniteAnywherePreferences Preferences { get; private set; }

        public PreferencesManager(string extensionsDataPath)
        {
            Directory.CreateDirectory(extensionsDataPath);

            preferencesPath = Path.Combine(
                extensionsDataPath,
                "preferences.json"
            );

            Load();
        }

        private void Load()
        {
            if (!File.Exists(preferencesPath))
            {
                Preferences = new PlayniteAnywherePreferences();
                Save();
                return;
            }

            try
            {
                using (var stream = File.OpenRead(preferencesPath))
                {
                    var serializer =
                        new DataContractJsonSerializer(
                            typeof(PlayniteAnywherePreferences)
                        );

                    Preferences =
                        serializer.ReadObject(stream)
                        as PlayniteAnywherePreferences;
                }

                if (Preferences == null)
                {
                    Preferences = new PlayniteAnywherePreferences();
                }
            }
            catch
            {
                Preferences = new PlayniteAnywherePreferences();
            }
        }

        public void Save()
        {
            var serializer =
                new DataContractJsonSerializer(
                    typeof(PlayniteAnywherePreferences)
                );

            using (var stream = File.Create(preferencesPath))
            {
                serializer.WriteObject(stream, Preferences);
            }
        }
    }
}