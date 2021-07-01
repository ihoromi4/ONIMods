using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;

namespace BatteriesChargeSensor
{
    public class BatteriesChargeSensorPatch : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Utils.AddBuildingToPlanScreen("Automation", BatteriesChargeSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Utils.AddBuildingToTechnology("AdvancedPowerRegulation", BatteriesChargeSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Localization), "Initialize")]
        class StringLocalisationPatch
        {
            public static void Postfix()
            {
                Utils.Localize(typeof(STRINGS));
            }
        }

    }
}
