using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace Bioreactor
{
	public class BioreactorConfig : IBuildingConfig
	{
		public const string ID = "Bioreactor";

		// storage
		public static readonly Tag COMPOST_TAG = GameTags.Compostable;
		public static float MAX_COMPOSTABLE_MASS_KG = 1800f;
		public static float MAX_GAS_MASS_KG = 50f;

		// production
		public static float COMPOSTABLE_CONSUME = 1.0f;
		public static float OXYGEN_PRODUCTION_FACTOR = 0.18f;
		public static float METHANE_PRODUCTION_FACTOR = 0.09f;
		public static float MIN_PRODUCTIVITY_MULTIPLAYER = 0.5f;

		// germs
		public static string DISEASE_ID = "FoodPoisoning";
		public static float MIN_WORKING_DISEASE_COUNT_KG = 50f;

		// temperature
		public static float MAX_SELFHEATING_TEMP = 273.15f + 50f;
		public static float SELFHEATING_SEC_PER_DEG = 100f;
		public static float MIN_TEMP = 273.15f + 15f;
		public static float OPTIMAL_TEMP = 273.15f + 55f;
		public static float EXTINCTION_TEMP = 273.15f + 75f;

		public override BuildingDef CreateBuildingDef()
		{
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				id: ID,
				width: 5,
				height: 3,
				anim: "gasstorage_kanim",
				hitpoints: BUILDINGS.HITPOINTS.TIER1,
				construction_time: BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER4,
				construction_mass: BUILDINGS.CONSTRUCTION_MASS_KG.TIER4,
				construction_materials: MATERIALS.RAW_METALS,
				melting_point: BUILDINGS.MELTING_POINT_KELVIN.TIER1,
				build_location_rule: BuildLocationRule.OnFloor,
				decor: BUILDINGS.DECOR.PENALTY.TIER3,
				noise: NOISE_POLLUTION.NOISY.TIER0);

			buildingDef.Floodable = false;
			buildingDef.Overheatable = true;
			buildingDef.MaterialCategory = MATERIALS.RAW_METALS;
			buildingDef.AudioCategory = "HollowMetal";
			buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			buildingDef.OverheatTemperature = 273.15f + 75f;
			buildingDef.ThermalConductivity = 100;

			buildingDef.PermittedRotations = PermittedRotations.FlipH;
			buildingDef.ObjectLayer = ObjectLayer.Building;

			buildingDef.OutputConduitType = ConduitType.Gas;
			buildingDef.UtilityOutputOffset = new CellOffset(1, 2);

			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
			go.AddOrGet<RequireOutputs>().ignoreFullPipe = true;

			// compostable storage
			Storage inStorage = go.AddOrGet<Storage>();
			inStorage.capacityKg = BioreactorConfig.MAX_COMPOSTABLE_MASS_KG;
			inStorage.showInUI = true;
			inStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
			inStorage.allowItemRemoval = false;
			inStorage.storageFilters = new List<Tag> {BioreactorConfig.COMPOST_TAG};

			// dirt storage
			Storage outStorage = go.AddComponent<Storage>();
			outStorage.capacityKg = BioreactorConfig.MAX_COMPOSTABLE_MASS_KG;
			outStorage.showInUI = true;
			outStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
			outStorage.allowItemRemoval = true;
			outStorage.storageFilters = new List<Tag> { SimHashes.Dirt.CreateTag() };

			ManualDeliveryKG manualDeliveryKg = go.AddOrGet<ManualDeliveryKG>();
			manualDeliveryKg.SetStorage(inStorage);
			manualDeliveryKg.requestedItemTag = BioreactorConfig.COMPOST_TAG;
			manualDeliveryKg.capacity = BioreactorConfig.MAX_COMPOSTABLE_MASS_KG;
			manualDeliveryKg.refillMass = BioreactorConfig.MAX_COMPOSTABLE_MASS_KG * 0.99f;
			manualDeliveryKg.minimumMass = 1f;
			manualDeliveryKg.choreTypeIDHash = Db.Get().ChoreTypes.FarmFetch.IdHash;

			ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
			conduitDispenser.storage = inStorage;
			conduitDispenser.conduitType = ConduitType.Gas;
			conduitDispenser.alwaysDispense = true;
			conduitDispenser.elementFilter = (SimHashes[])null;

			ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
			elementConverter.SetStorage(inStorage);
			elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
			{
				new ElementConverter.ConsumedElement(BioreactorConfig.COMPOST_TAG, COMPOSTABLE_CONSUME)
			};
			elementConverter.outputElements = new ElementConverter.OutputElement[3]
			{
				new ElementConverter.OutputElement(COMPOSTABLE_CONSUME * (1 - OXYGEN_PRODUCTION_FACTOR - METHANE_PRODUCTION_FACTOR), SimHashes.Dirt, 273.15f, storeOutput: true),
				new ElementConverter.OutputElement(COMPOSTABLE_CONSUME * OXYGEN_PRODUCTION_FACTOR, SimHashes.ContaminatedOxygen, 273.15f, storeOutput: true),
				new ElementConverter.OutputElement(COMPOSTABLE_CONSUME * METHANE_PRODUCTION_FACTOR, SimHashes.Methane, 273.15f, storeOutput: true)
			};

			BioreactorEmpty workable = go.AddOrGet<BioreactorEmpty>();
			workable.workTime = 10f;

			Bioreactor bioreactor = go.AddOrGet<Bioreactor>();
			bioreactor.inStorage = inStorage;
			bioreactor.outStorage = outStorage;
			bioreactor.converter = elementConverter;

			Prioritizable.AddRef(go);
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
		}
	}
}