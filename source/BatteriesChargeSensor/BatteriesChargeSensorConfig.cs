using System.Collections.Generic;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace BatteriesChargeSensor
{
	public class BatteriesChargeSensorConfig : IBuildingConfig
	{
		public const string ID = "BatteriesChargeSensor";
		protected const string kanim = "wattage_sensor_kanim";

		public override BuildingDef CreateBuildingDef()
		{
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				id: ID,
				width: 1,
				height: 1,
				anim: kanim,
				hitpoints: BUILDINGS.HITPOINTS.TIER1,
				construction_time: BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER0,
				construction_mass: BUILDINGS.CONSTRUCTION_MASS_KG.TIER0,
				construction_materials: MATERIALS.RAW_METALS,
				melting_point: BUILDINGS.MELTING_POINT_KELVIN.TIER1,
				build_location_rule: BuildLocationRule.Anywhere,
				decor: BUILDINGS.DECOR.PENALTY.TIER0,
				noise: NOISE_POLLUTION.NOISY.TIER0);

			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.Entombable = false;
			buildingDef.MaterialCategory = MATERIALS.REFINED_METALS;
			buildingDef.AudioCategory = "Metal";
			buildingDef.ViewMode = OverlayModes.Logic.ID;
			buildingDef.SceneLayer = Grid.SceneLayer.Building;
			buildingDef.AlwaysOperational = true;
			buildingDef.RequiresPowerOutput = true;
			buildingDef.UseWhitePowerOutputConnectorColour = true;
			buildingDef.LogicOutputPorts = new List<LogicPorts.Port>()
			{
				LogicPorts.Port.OutputPort(BatteriesChargeSensor.PORT_ID, new CellOffset(0, 0), (string) STRINGS.BUILDINGS.PREFABS.BATTERIESCHARGESENSOR.LOGIC_PORT, (string) STRINGS.BUILDINGS.PREFABS.BATTERIESCHARGESENSOR.LOGIC_PORT_ACTIVE, (string) STRINGS.BUILDINGS.PREFABS.BATTERIESCHARGESENSOR.LOGIC_PORT_INACTIVE, true)
			};
			SoundEventVolumeCache.instance.AddVolume(BatteriesChargeSensorConfig.kanim, "PowerSwitch_on", TUNING.NOISE_POLLUTION.NOISY.TIER3);
			SoundEventVolumeCache.instance.AddVolume(BatteriesChargeSensorConfig.kanim, "PowerSwitch_off", TUNING.NOISE_POLLUTION.NOISY.TIER3);
			GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, BatteriesChargeSensorConfig.ID);

			return buildingDef;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits);
			BatteriesChargeSensor batteriesChargeSensor = go.AddOrGet<BatteriesChargeSensor>();
			batteriesChargeSensor.manuallyControlled = false;
		}
	}
}