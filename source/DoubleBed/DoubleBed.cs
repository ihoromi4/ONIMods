using System.Collections.Generic;
using UnityEngine;
using TUNING;
using Klei.AI;

namespace DoubleBed
{
    class DoubleBed: KMonoBehaviour, IGameObjectEffectDescriptor, IBasicBuilding
    {
        public string[] effects;
        public Vector3[] choreOffset = new Vector3[2]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 1f, 0f)
        };
        public Vector3[] animOffset = new Vector3[2]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0.3f, 0f)
        };
        public Sleepable[] sleepables;
        private static Dictionary<string, string> roomSleepingEffects = new Dictionary<string, string>()
        {
            {
                "Barracks",
                "BarracksStamina"
            },
            {
                "Bedroom",
                "BedroomStamina"
            }
        };

        protected override void OnSpawn()
        {
            Components.BasicBuildings.Add(this);

            sleepables = new Sleepable[choreOffset.Length];
            for(int i = 0; i < choreOffset.Length; i++)
            {
                GameObject locator = ChoreHelpers.CreateSleepLocator(Grid.CellToPosCBC(Grid.PosToCell(this), Grid.SceneLayer.BuildingUse) + choreOffset[i]);
                Building building = locator.AddOrGet<Building>();
                building.Def = BuildingTemplates.CreateBuildingDef(DoubleBedConfig.ID, 2, 1, "bedlg_kanim", 10, 10f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_MINERALS, 1600f, BuildLocationRule.OnFloor, BUILDINGS.DECOR.NONE, NOISE_POLLUTION.NONE);
                //building.Def = DoubleBedConfig.CreateBuildingDef();
                //locator.AddComponent<BuildingComplete>();

                KSelectable kselectable = locator.AddOrGet<KSelectable>();
                kselectable.IsSelectable = false;
                kselectable.SetName(building.Def + string.Format(" ({0})", i));

                Sleepable sleepable = locator.AddOrGet<Sleepable>();
                sleepable.overrideAnims = new KAnimFile[1]
                {
                    Assets.GetAnim("anim_sleep_bed_kanim")
                };
                sleepable.workLayer = Grid.SceneLayer.Building;
                sleepable.OnWorkableEventCB += ((workable, evt) => OnWorkableEvent(sleepable, evt));

                Ownable ownable = locator.AddOrGet<Ownable>();
                ownable.slotID = Db.Get().AssignableSlots.Bed.Id;

                DoubleBedDummy dummy = locator.AddOrGet<DoubleBedDummy>();
                sleepable.AnimOffset = animOffset[i];

                locator.AddOrGet<KPrefabID>();

                sleepables[i] = sleepable;
            }

            ToggleLedder(true);
        }

        protected override void OnCleanUp()
        {
            Components.BasicBuildings.Remove(this);

            for (int i = 0; i < choreOffset.Length; i++)
            {
                Util.KDestroyGameObject(sleepables[i]);
                sleepables[i] = null;
            }

            ToggleLedder(false);
        }

        private void ToggleLedder(bool flag)
        {
            int cell = Grid.PosToCell(this);
            Grid.HasLadder[cell] = flag;
        }

        private void OnWorkableEvent(Sleepable sleepable, Workable.WorkableEvent workable_event)
        {
            if (workable_event == Workable.WorkableEvent.WorkStarted)
            {
                AddEffects(sleepable.worker);
            }
            else if (workable_event == Workable.WorkableEvent.WorkStopped)
            {
                RemoveEffects(sleepable.worker);
                Debug.Log("Double Bed Unassign");
            }
        }

        private void AddEffects(Worker worker)
        {
            if (this.effects != null)
            {
                foreach (string effect in this.effects)
                    worker.GetComponent<Effects>().Add(effect, false);
            }

            Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(this.gameObject);
            if (roomOfGameObject == null)
                return;

            RoomType roomType = roomOfGameObject.roomType;
            foreach (KeyValuePair<string, string> roomSleepingEffect in roomSleepingEffects)
            {
                if (roomSleepingEffect.Key == roomType.Id)
                    worker.GetComponent<Effects>().Add(roomSleepingEffect.Value, false);
            }
            roomType.TriggerRoomEffects(this.GetComponent<KPrefabID>(), worker.GetComponent<Effects>());
        }

        private void RemoveEffects(Worker worker)
        {
            if (worker == null)
                return;

            if (this.effects != null)
            {
                foreach (string effect in this.effects)
                    worker.GetComponent<Effects>().Remove(effect);
            }
            foreach (KeyValuePair<string, string> roomSleepingEffect in roomSleepingEffects)
                worker.GetComponent<Effects>().Remove(roomSleepingEffect.Value);
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            List<Descriptor> descs = new List<Descriptor>();
            if (this.effects != null)
            {
                foreach (string effect in this.effects)
                {
                    if (effect != null && effect != "")
                        Effect.AddModifierDescriptions(this.gameObject, descs, effect);
                }
            }
            return descs;
        }
    }
}
