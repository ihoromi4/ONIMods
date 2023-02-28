using TUNING;
using UnityEngine;

namespace MegaBattery
{
    class MegaBatteryConfig : IBuildingConfig
    {
        public const string ID = "BatteryLarge";
        public const string kanim_name = "rocket_battery_pack_kanim";
        public static readonly Tag TagBattery = TagManager.Create("Battery");

        public override string[] GetDlcIds() => DlcManager.AVAILABLE_EXPANSION1_ONLY;

        public override BuildingDef CreateBuildingDef()
        {
            LocString.CreateLocStringKeys(typeof(MegaBatteryStrings.BUILDINGS));

            float[] mass = new float[1] { 500f };
            string[] materials = MATERIALS.ALL_METALS;
            EffectorValues noise = NOISE_POLLUTION.NOISY.TIER1;
            EffectorValues decor = BUILDINGS.DECOR.PENALTY.TIER3;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 3, 2, kanim_name, 60, 60f, mass, materials, 800f, BuildLocationRule.OnFloorOrBuildingAttachPoint, decor, noise);
            buildingDef.ExhaustKilowattsWhenActive = 0.25f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;
            buildingDef.Entombable = false;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.RequiresPowerOutput = true;
            buildingDef.UseWhitePowerOutputConnectorColour = true;

            buildingDef.DefaultAnimState = "grounded";
            buildingDef.ForegroundLayer = Grid.SceneLayer.Front;
            buildingDef.PowerOutputOffset = new CellOffset(-1, 0);
            buildingDef.PowerInputOffset = new CellOffset(-1, 0);

            buildingDef.attachablePosition = new CellOffset(0, 0);
            buildingDef.AttachmentSlotTag = TagBattery;
            buildingDef.ObjectLayer = ObjectLayer.Building;
            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            go.AddComponent<RequireInputs>();
            go.AddOrGet<BuildingAttachPoint>().points = new BuildingAttachPoint.HardPoint[1]
            {
                new BuildingAttachPoint.HardPoint(new CellOffset(0, 2), TagBattery, null)
            };
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Battery battery = go.AddOrGet<Battery>();
            battery.capacity = 100000f;
            battery.joulesLostPerSecond = 4.166666875f;
            battery.powerSortOrder = 1000;
            go.AddOrGetDef<PoweredActiveController.Def>();

            KBatchedAnimController component1 = go.GetComponent<KBatchedAnimController>();
            component1.initialMode = KAnim.PlayMode.Loop;
        }
    }
}
