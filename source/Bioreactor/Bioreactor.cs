using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace Bioreactor
{
	public class Bioreactor : StateMachineComponent<Bioreactor.SMInstance>, IGameObjectEffectDescriptor
	{
		public Storage inStorage;
		public Storage outStorage;
		public ElementConverter converter;

#pragma warning disable 649
		[MyCmpGet] private readonly Operational operational;
#pragma warning restore 649

		public float WorkSpeedMultiplier;
		private static StatusItem EfficencyStatus;
		private static StatusItem GasStorageFullStatus;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();

			EfficencyStatus = new StatusItem("PRODUCTIVITY_STATUS", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, true, OverlayModes.None.ID)
				.SetResolveStringCallback((str, data) => 
				{
					Bioreactor bioreactor = (Bioreactor)data;
					str = str.Replace(STRINGS.PRODUCTIVITY_KEY, (bioreactor.WorkSpeedMultiplier * 100f).ToString("0.00"));
					return str;
				});
			GasStorageFullStatus = new StatusItem("GASS_STORAGE_FULL_STATUS", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Bad, true, OverlayModes.None.ID);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			smi.StartSM();
		}

		public bool showDescriptors = true;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			List<Descriptor> descriptorList = new List<Descriptor>();
			if (!this.showDescriptors)
				return descriptorList;

			Descriptor descriptor = new Descriptor();
			string text = Strings.Get("STRINGS.BUILDINGS.PREFABS.BIOREACTOR.DESCRIPTORS.TEXT");
			text = text.Replace("{OPTIMAL_TEMP}", GameUtil.GetFormattedTemperature(BioreactorConfig.OPTIMAL_TEMP));
			string tooltip = Strings.Get("STRINGS.BUILDINGS.PREFABS.BIOREACTOR.DESCRIPTORS.TOOLTIP");
			descriptor.SetupDescriptor(text, tooltip, Descriptor.DescriptorType.Effect);
			descriptorList.Add(descriptor);

			return descriptorList;
		}

		public class SMInstance : GameStateMachine<States, SMInstance, Bioreactor, object>.GameInstance
		{
			private readonly Operational operational;
			private readonly ElementConverter converter;
			private readonly Storage storage;
			public Chore emptyChore;
			private Guid statusGUID;

			public SMInstance(Bioreactor master) : base(master)
			{
				operational = master.GetComponent<Operational>();
				converter = master.GetComponent<ElementConverter>();
				storage = master.GetComponent<Storage>();

				converter.Subscribe(-1697596308, OnConvertElements);
			}

			public void CreateEmptyChore()
			{
				if (this.emptyChore != null)
					this.emptyChore.Cancel("dupe");

				BioreactorEmpty component = this.master.GetComponent<BioreactorEmpty>();
				emptyChore = new WorkChore<BioreactorEmpty>(Db.Get().ChoreTypes.EmptyStorage, component, on_complete: this.OnEmptyComplete, ignore_building_assignment: true);
			}

			public void CancelEmptyChore()
			{
				if (this.emptyChore == null) return;
				this.emptyChore.Cancel("Cancelled");
				this.emptyChore = null;
			}

			private void OnEmptyComplete(Chore chore)
			{
				this.emptyChore = null;
				this.master.outStorage.DropAll(true);
			}

			public void AddStatusItem(StatusItem status)
			{
				statusGUID = master.GetComponent<KSelectable>().AddStatusItem(status, (object) master);
			}

			public void RemoveStatusItem()
			{
				master.GetComponent<KSelectable>().RemoveStatusItem(statusGUID);
			}

			public bool HasEnoughMass(Tag tag) => converter.HasEnoughMass(tag);

			public bool IsOperational => operational.IsOperational;

			public bool HasCompostable() => storage.GetAmountAvailable(GameTags.Compostable) > 0.0f;

			public bool IsGassesStorageFull() => (master.inStorage.GetMassAvailable(SimHashes.ContaminatedOxygen) + master.inStorage.GetMassAvailable(SimHashes.Methane)) >= BioreactorConfig.MAX_GAS_MASS_KG;

			public bool GermsProcess(float dt)
			{
				foreach (GameObject gameObject in this.storage.items)
				{
					if (!gameObject.HasTag(GameTags.Compostable)) continue;
					var element = gameObject.GetComponent<PrimaryElement>();
					byte diseaseIndex = Db.Get().Diseases.GetIndex(BioreactorConfig.DISEASE_ID);

					if (element.DiseaseIdx == diseaseIndex)
					{
						if (element.DiseaseCount / element.Mass < 5e5)
						{
							int delta = (int) ((float)element.DiseaseCount * 0.01f * dt);
							element.AddDisease(element.DiseaseIdx, delta, "Bioreactor.GermsProcess");
						}

						bool enoughDisease = element.DiseaseCount / element.Mass > BioreactorConfig.MIN_WORKING_DISEASE_COUNT_KG;
						//master.converter.enabled = !enoughDisease;
						if (enoughDisease) return true;
					}
					else
					{
						int delta = (int) (-(float)element.DiseaseCount * 0.99f);
						element.ModifyDiseaseCount(delta, "Bioreactor.GermsProcess");

						if (element.DiseaseCount < 50)
							element.AddDisease(diseaseIndex, (int)(20 * element.Mass), "Bioreactor.GermsProcess");
					}
				}
				return false;
			}

			public float GetTemperatureProductivityMultiplier()
			{
				GameObject item = storage.FindFirst(GameTags.Compostable);
				PrimaryElement element = item.GetComponent<PrimaryElement>();
				float temperature_multiplier = (element.Temperature - BioreactorConfig.MIN_TEMP) / (BioreactorConfig.OPTIMAL_TEMP - BioreactorConfig.MIN_TEMP);
				if (element.Temperature > BioreactorConfig.OPTIMAL_TEMP)
					temperature_multiplier = (BioreactorConfig.EXTINCTION_TEMP - element.Temperature) / (BioreactorConfig.EXTINCTION_TEMP - BioreactorConfig.OPTIMAL_TEMP);
				return Mathf.Clamp(temperature_multiplier, 0f, 1f);
			}

			public float GetMassProductivityMultiplier()
			{
				float compostable_mass_kg = storage.GetAmountAvailable(GameTags.Compostable);
				float mass_multiplier = compostable_mass_kg / BioreactorConfig.MAX_COMPOSTABLE_MASS_KG;
				return Mathf.Clamp(mass_multiplier, 0.1f, 1f);
			}

			public void UpdateProductivity()
			{
				float temperature_multiplier = GetTemperatureProductivityMultiplier();
				//float mass_multiplier = GetMassProductivityMultiplier();
				//float multiplier = temperature_multiplier * mass_multiplier;
				float multiplier = Mathf.Clamp(temperature_multiplier, BioreactorConfig.MIN_PRODUCTIVITY_MULTIPLAYER, 1.0f);
				master.converter.SetWorkSpeedMultiplier(multiplier);
				master.WorkSpeedMultiplier = multiplier;
			}

			public void OnConvertElements(object go)
			{
				master.inStorage.Transfer(master.outStorage, SimHashes.Dirt.CreateTag(), Mathf.Infinity, hide_popups: true);
			}

			public void DoStorageHeating(float dt)
			{
				foreach (GameObject gameObject in this.storage.items)
				{
					if (!gameObject.HasTag(GameTags.Compostable)) continue;
					var element = gameObject.GetComponent<PrimaryElement>();
					float kilowatts_per_kg = element.Element.specificHeatCapacity / BioreactorConfig.SELFHEATING_SEC_PER_DEG;
					float kilowatts = kilowatts_per_kg * element.Mass * dt;
					if (element.Temperature <= 0) continue;
					if (element.Temperature > BioreactorConfig.MAX_SELFHEATING_TEMP) continue;
					GameUtil.DeltaThermalEnergy(element, kilowatts, BioreactorConfig.MAX_SELFHEATING_TEMP);
				}
			}

			public void Update(float dt)
			{
				//GermsProcess(dt);
				UpdateProductivity();
				DoStorageHeating(dt);
			}
		}

		public class States : GameStateMachine<States, SMInstance, Bioreactor>
		{
			public State NotOperational;
			public State NoCompostable;
			public State GotCompostable;
			public State GassesStorageFull;
			public State ProcessingCompostable;

			public override void InitializeStates(out BaseState defaultState)
			{
				defaultState = NotOperational;

				root
					.EventTransition(GameHashes.OperationalChanged, NotOperational, smi => !smi.IsOperational);

				NotOperational
					.QueueAnim("off")
					.EventTransition(GameHashes.OperationalChanged, NoCompostable, smi => smi.IsOperational);

				NoCompostable
					.QueueAnim("off")
					.Enter(smi => smi.master.operational.SetActive(false))
					.EventTransition(GameHashes.OnStorageChange, GotCompostable, smi => smi.HasEnoughMass(GameTags.Compostable));

				GotCompostable
					.QueueAnim("on")
					.Enter(smi => smi.master.operational.SetActive(true))
					.OnAnimQueueComplete(ProcessingCompostable);

				ProcessingCompostable
					.Enter(smi => { smi.master.operational.SetActive(true); smi.AddStatusItem(Bioreactor.EfficencyStatus); })
					.Exit(smi => { smi.master.operational.SetActive(false); smi.RemoveStatusItem(); })
					.QueueAnim("working", true)
					.EventTransition(GameHashes.OnStorageChange, NoCompostable, smi => !smi.HasEnoughMass(GameTags.Compostable))
					.EventTransition(GameHashes.OnStorageChange, GassesStorageFull, smi => smi.IsGassesStorageFull())
					.Update("ProcessingCompostable", (smi, dt) => { smi.Update(dt); }, UpdateRate.SIM_1000ms);

				GassesStorageFull
					.Enter(smi => { smi.AddStatusItem(Bioreactor.GasStorageFullStatus); })
					.Exit(smi => { smi.RemoveStatusItem(); })
					.QueueAnim("off")
					.EventTransition(GameHashes.OnStorageChange, ProcessingCompostable, smi => !smi.IsGassesStorageFull());
			}
		}
	}
}