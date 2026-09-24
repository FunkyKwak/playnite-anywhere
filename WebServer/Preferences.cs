using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Playnite.SDK;
using Swan;

namespace PlayniteAnywhere
{
    public class PlayniteAnywherePreferences
    {
        public string GroupBy { get; set; } = "None";
        public Dictionary<string, bool> CollapsedGroups { get; set; } = new Dictionary<string, bool>();
    }


    public class GroupStateRequest
    {
        public string Name { get; set; }
        public bool Collapsed { get; set; }
    }

    public class PreferencesManager
    {
        private static readonly ILogger logger = LogManager.GetLogger();
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

                if (Preferences.CollapsedGroups == null)
                {
                    Preferences.CollapsedGroups = new Dictionary<string, bool>();
                }
            }
            catch
            {
                Preferences = new PlayniteAnywherePreferences();
            }
        }

        public void SetGroupCollapsed(string groupName, bool collapsed)
        {
            Preferences.CollapsedGroups[groupName] = collapsed;
            Save();
        }
        public bool IsGroupCollapsed(string groupName)
        {
            bool collapsed;

            if (Preferences.CollapsedGroups.TryGetValue(groupName, out collapsed))
            {
                return collapsed;
            }

            return false;
        }

        public void Save()
        {
            logger.Info($"Saving preferences in : {preferencesPath}");
            logger.Info($"{Preferences.ToJson()}");

            if (Preferences == null)
            {
                Preferences = new PlayniteAnywherePreferences();
            }

            if (Preferences.CollapsedGroups == null)
            {
                Preferences.CollapsedGroups = new Dictionary<string, bool>();
            }

            var serializer =
                new DataContractJsonSerializer(
                    typeof(PlayniteAnywherePreferences)
                );

            using (var stream = File.Create(preferencesPath))
            {
                serializer.WriteObject(stream, Preferences);
                logger.Info($"Preferences saved in : {preferencesPath}");
            }
        }
    }
}