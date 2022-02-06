using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace PassiveGasVentInput
{
    class PassiveGasVentInputConfig : IBuildingConfig
    {
        public const string ID = "PassiveGasVentInput";
		protected const string kanim = "passive_gas_vent_input_kanim";

		public override BuildingDef CreateBuildingDef()
		{
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				id: ID,
				width: 1,
				height: 1,
				anim: kanim,
				hitpoints: BUILDINGS.HITPOINTS.TIER1,
				construction_time: BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER1,
				construction_mass: BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
				construction_materials: MATERIALS.RAW_METALS,
				melting_point: BUILDINGS.MELTING_POINT_KELVIN.TIER1,
				build_location_rule: BuildLocationRule.Anywhere,
				decor: BUILDINGS.DECOR.PENALTY.TIER1,
				noise: NOISE_POLLUTION.NONE);

			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.MaterialCategory = MATERIALS.RAW_METALS;
			buildingDef.AudioCategory = "Metal";
			buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			buildingDef.SceneLayer = Grid.SceneLayer.Building;
			buildingDef.OutputConduitType = ConduitType.Gas;
			buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
			buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
			buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, PassiveGasVentInputConfig.ID);
			SoundEventVolumeCache.instance.AddVolume("ventgas_kanim", "GasVent_clunk", NOISE_POLLUTION.NOISY.TIER0);

			return buildingDef;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
			go.AddOrGet<LoopingSounds>();
			go.AddOrGet<LogicOperationalController>();
			go.GetComponent<KBatchedAnimController>().initialAnim = "on";

			Storage storage = go.AddOrGet<Storage>();

			var elementConsumer = go.AddOrGet<ElementConsumer>();
			elementConsumer.showDescriptor = false;
			elementConsumer.configuration = ElementConsumer.Configuration.AllGas;
			elementConsumer.capacityKG = PassiveGasVentInputSettings.Instance.MaximumFlow;
			elementConsumer.consumptionRate = PassiveGasVentInputSettings.Instance.MinimumFlow;
			elementConsumer.storeOnConsume = true;  // true otherwise removes gas
			elementConsumer.showInStatusPanel = false;
			elementConsumer.consumptionRadius = 1;

			var conduitDispenser = go.AddOrGet<ConduitDispenser>();
			conduitDispenser.conduitType = ConduitType.Gas;
			conduitDispenser.alwaysDispense = true;
			conduitDispenser.elementFilter = null;

			PassiveGasVentInput passiveGasVentInput = go.AddOrGet<PassiveGasVentInput>();
		}
	}
}
