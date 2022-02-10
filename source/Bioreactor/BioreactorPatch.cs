using HarmonyLib;

namespace Bioreactor
{
    public class BioreactorPatch : KMod.UserMod2
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
                Utils.AddBuildingToPlanScreen("Refining", BioreactorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Utils.AddBuildingToTechnology("FoodRepurposing", BioreactorConfig.ID);
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
