using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Harmony;
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
            var tech_grouping = Traverse.Create(typeof(Techs))?.Field("TECH_GROUPING")?.GetValue<Dictionary<string, string[]>>();
            bool isVanilla = tech_grouping != null;
            if (isVanilla)
            {
                if (tech_grouping.ContainsKey(techId))
                {
                    List<string> techList = new List<string>(tech_grouping[techId]) { buildingId };
                    tech_grouping[techId] = techList.ToArray();
                }
                else
                    Debug.LogWarning($"{Utils.modInfo.assemblyName}: Could not find '{techId}' tech in TECH_GROUPING.");
            }
            else
            {
                Tech tech = Db.Get()?.Techs.TryGet(techId);
                if (tech != null)
                {
                    Traverse.Create(tech)?.Field("unlockedItemIDs")?.GetValue<List<string>>()?.Add(buildingId);
                }
                else
                    Debug.LogWarning($"{Utils.modInfo.assemblyName}: Could not find '{techId}' tech.");
            }
        }

        public static void AddBuildingToPlanScreen(HashedString category, string buildingId)
        {
            int index = BUILDINGS.PLANORDER.FindIndex(x => x.category == category);
            if (index == -1)
            {
                Debug.LogWarning($"{modInfo.assemblyName}: Could not find '{category}' category in the building menu.");
                return;
            }

            var planOrderList = Traverse.Create(BUILDINGS.PLANORDER[index])?.Field("data")?.GetValue<List<string>>();
            if (planOrderList == null)
            {
                Debug.LogWarning($"{modInfo.assemblyName}: Could not add '{buildingId}' to the building menu.");
                return;
            }
            planOrderList.Add(buildingId);
        }

        public static StatusItem CreateStatusItem(string id, string prefix, string icon, StatusItem.IconType icon_type, NotificationType notification_type, bool allow_multiples, HashedString render_overlay, bool showWorldIcon = true, int status_overlays = 129022)
        {
            Type[] types_vanilla = new Type[] { typeof(string), typeof(string), typeof(string), typeof(StatusItem.IconType), typeof(NotificationType), typeof(bool), typeof(HashedString), typeof(bool), typeof(int)};
            ConstructorInfo constructorInfo = typeof(StatusItem).GetConstructor(types_vanilla);
            if (constructorInfo != null)
            {
                // vanilla
                object[] parameters = new object[] { id, prefix, icon, icon_type, notification_type, allow_multiples, render_overlay, showWorldIcon, status_overlays };
                return (StatusItem)constructorInfo.Invoke(parameters);
            }

            Type[] types_dlc = new Type[] { typeof(string), typeof(string), typeof(string), typeof(StatusItem.IconType), typeof(NotificationType), typeof(bool), typeof(HashedString), typeof(bool), typeof(int), typeof(Func<string, object, string>) };
            constructorInfo = typeof(StatusItem).GetConstructor(types_dlc);
            if (constructorInfo != null)
            {
                // dlc
                object[] parameters = new object[] { id, prefix, icon, icon_type, notification_type, allow_multiples, render_overlay, showWorldIcon, status_overlays, null };
                return (StatusItem)constructorInfo.Invoke(parameters);
            }

            return null;
        }
    }
}
