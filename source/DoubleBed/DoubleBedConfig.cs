using TUNING;
using UnityEngine;

namespace DoubleBed
{
    class DoubleBedConfig : IBuildingConfig
    {
        public static string ID = "DoubleBed";
        protected const string kanim = "doublebed_kanim";
        //protected const string kanim = "bedlg_kanim";

        public override BuildingDef CreateBuildingDef()
        {
            float[] mass = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] materials = MATERIALS.RAW_MINERALS;
            EffectorValues decor = BUILDINGS.DECOR.NONE;
            EffectorValues noise = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 3, kanim, 10, 10f, mass, materials, 1600f, BuildLocationRule.OnFloor, decor, noise);
            buildingDef.Overheatable = false;
            buildingDef.AudioCategory = "Metal";
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.Bed);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KAnimControllerBase>().initialAnim = "off";

            DoubleBed bed = go.AddOrGet<DoubleBed>();
            bed.effects = new string[2] {
                "BedStamina",
                "BedHealth"
            };
        }
    }
}
