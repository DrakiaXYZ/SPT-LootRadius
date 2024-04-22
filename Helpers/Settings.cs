using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace DrakiaXYZ.LootRadius.Helpers
{
    internal class Settings
    {
        public const string GeneralSectionTitle = "1. General";

        public static ConfigFile Config;

        public static ConfigEntry<float> LootRadius;

        public static List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

        public static void Init(ConfigFile Config)
        {
            Settings.Config = Config;

            ConfigEntries.Add(LootRadius = Config.Bind(
                GeneralSectionTitle,
                "Loot Radius",
                1.5f,
                new ConfigDescription(
                    "The distance to include loot from",
                    new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes { })));

            RecalcOrder();
        }

        private static void RecalcOrder()
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = ConfigEntries.Count;
            foreach (var entry in ConfigEntries)
            {
                ConfigurationManagerAttributes attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
                if (attributes != null)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
