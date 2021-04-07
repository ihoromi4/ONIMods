using System;
using System.IO;
using System.Reflection;
using Harmony;
using CaiLib.Utils;
using static CaiLib.Utils.BuildingUtils;

namespace BatteriesChargeSensor
{
    public class Patches
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Automation, BatteriesChargeSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingToTechnology(GameStrings.Technology.Power.AdvancedPowerRegulation, BatteriesChargeSensorConfig.ID);
            }
        }


        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("Initialize")]
        class StringLocalisationPatch
        {
            public static void Postfix()
            {
                Localize();
            }
        }

        private static void Localize()
        {
            InitLocalization(typeof(STRINGS));
        }

        public static void InitLocalization(Type locstring_tree_root, string filename_prefix = "", bool writeStringsTemplate = false)
        {
            ModUtil.RegisterForTranslation(locstring_tree_root);

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyName = assembly.GetName().Name;
            string rootDirectory = Path.GetDirectoryName(assembly.Location);
            string langDirectory = Path.Combine(rootDirectory, "translations");

            Localization.Locale locale = Localization.GetLocale();
            if (locale != null)
            {
                try
                {
                    string langFile = Path.Combine(langDirectory, filename_prefix + locale.Code + ".po");
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
            STRINGS.DoReplacement();
        }
    }
}
