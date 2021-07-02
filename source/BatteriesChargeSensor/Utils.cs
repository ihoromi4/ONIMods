using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Database;
using TUNING;

namespace BatteriesChargeSensor
{
    public static class Utils
    {
        public class ModInfo
        {
            public readonly string assemblyName;
            public readonly string rootDirectory;
            public readonly string langDirectory;
            public readonly string spritesDirectory;
            public readonly string version;
            public ModInfo()
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                assemblyName = assembly.GetName().Name;
                rootDirectory = Path.GetDirectoryName(assembly.Location);
                langDirectory = Path.Combine(rootDirectory, "translations");
                spritesDirectory = Path.Combine(rootDirectory, "sprites");
                version = assembly.GetName().Version.ToString();
            }
        }

        private static ModInfo _modinfo;

        public static ModInfo modInfo
        {
            get
            {
                if (_modinfo == null)
                {
                    _modinfo = new ModInfo();
                }
                return _modinfo;
            }
        }

        public static void AddBuildingToTechnology(string techId, string buildingId)
        {
            Tech tech = Db.Get()?.Techs.TryGet(techId);

            if (tech != null)
                tech.unlockedItemIDs.Add(buildingId);
            else
                Debug.LogWarning($"{Utils.modInfo.assemblyName}: Could not find '{techId}' tech.");
        }

        public static void AddBuildingToPlanScreen(HashedString category, string buildingId)
        {
            ModUtil.AddBuildingToPlanScreen(category, buildingId);
        }

        public static void Localize(Type root)
        {
            ModUtil.RegisterForTranslation(root);

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyName = assembly.GetName().Name;
            string rootDirectory = Path.GetDirectoryName(assembly.Location);
            string langDirectory = Path.Combine(rootDirectory, "translations");

            Localization.Locale locale = Localization.GetLocale();

            if (locale != null)
            {
                try
                {
                    string langFile = Path.Combine(langDirectory, locale.Code + ".po");
                    if (File.Exists(langFile))
                    {
                        Debug.Log($"{assemblyName}: Localize file found " + langFile);
                        Localization.OverloadStrings(Localization.LoadStringsFile(langFile, false));
                    }
                }
                catch
                {
                    Debug.LogWarning($"{assemblyName} Failed to load localization.");
                }
            }

            LocString.CreateLocStringKeys(root, "");
        }
    }
}
