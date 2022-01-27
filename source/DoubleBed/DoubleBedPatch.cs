using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace DoubleBed
{
    public class DoubleBedPatch : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Utils.AddBuildingToPlanScreen("Furniture", DoubleBedConfig.ID, 1);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Utils.AddBuildingToTechnology("InteriorDecor", DoubleBedConfig.ID);
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

        [HarmonyPatch(typeof(Workable), nameof(Workable.GetWorkOffset))]
        class WorkablePatch
        {
            public static bool Prefix(Workable __instance, ref Vector3 __result)
            {
                __result = __instance.AnimOffset;
                return false;
            }
        }

        [HarmonyPatch(typeof(SleepChore.States), nameof(SleepChore.States.InitializeStates))]
        public static class SleepChorePatch
        {
            public static void Postfix(SleepChore.States __instance)
            {
                __instance.success.Exit((SleepChore.StatesInstance smi) =>
                {
                    var minion = smi.sm.sleeper.Get(smi).GetComponent<MinionIdentity>();
                    if (minion == null) return;
                    Ownables ownables = minion.GetSoleOwner();
                    if (ownables == null) return;
                    AssignableSlotInstance slot = ownables.GetSlot(Db.Get().AssignableSlots.Bed);
                    if (slot == null) return;
                    if (slot.assignable?.GetComponent<DoubleBedDummy>() == null) return;
                    slot.Unassign(false);
                });
            }
        }
    }
}
