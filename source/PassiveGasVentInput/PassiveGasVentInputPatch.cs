using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace PassiveGasVentInput
{
    public class PassiveGasVentInputPatch : KMod.UserMod2
    {
        internal static readonly PassiveGasVentInputSettings DefaultSettings = new PassiveGasVentInputSettings();

        internal POptions Options { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            PUtil.InitLibrary();

            Options = new POptions();
            Options.RegisterOptions(this, typeof(PassiveGasVentInputSettings));

            base.OnLoad(harmony);
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Utils.AddBuildingToPlanScreen("HVAC", PassiveGasVentInputConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Utils.AddBuildingToTechnology("GasPiping", PassiveGasVentInputConfig.ID);
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
