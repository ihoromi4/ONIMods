using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSerialization;
using STRINGS;

namespace BatteriesChargeSensor
{
    class BatteriesChargeSensor: Switch, ISim200ms, IActivationRangeTarget
    {
        public static readonly HashedString PORT_ID = "BatteriesChargeSensorLogicPort";
        private int powerCell;
        [Serialize]
        private int activateValue;
        [Serialize]
        private int deactivateValue = 100;
        [MyCmpGet]
        private LogicPorts logicPorts;
        private static StatusItem StoredEnergyStatus;
        private Guid statusGUID;
        private float storedEnergy;
        private float maxStoredEnergy;
        private bool wasOn;
        [MyCmpAdd] private CopyBuildingSettings copyBuildingSettings;
        private static readonly EventSystem.IntraObjectHandler<BatteriesChargeSensor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<BatteriesChargeSensor>((component, data) => component.OnCopySettings(data));

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            this.Subscribe<BatteriesChargeSensor>(-905833192, BatteriesChargeSensor.OnCopySettingsDelegate);
            StoredEnergyStatus = Utils.CreateStatusItem("STOREDENERGY_STATUS", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, true, OverlayModes.Power.ID);
            StoredEnergyStatus?.SetResolveStringCallback(ResolveStringCallback);
            if (StoredEnergyStatus == null) Debug.LogError($"{Utils.modInfo.assemblyName}: StoredEnergyStatus == null");
        }

        public string ResolveStringCallback(string str, object data)
        {
            BatteriesChargeSensor sensor = (BatteriesChargeSensor) data;
            str = str.Replace("{JoulesAvailable}", GameUtil.GetFormattedJoules(sensor.storedEnergy));
            str = str.Replace("{JoulesCapacity}", GameUtil.GetFormattedJoules(sensor.maxStoredEnergy));
            return str;
        }

        private void OnCopySettings(object data)
        {
            BatteriesChargeSensor component = ((GameObject)data).GetComponent<BatteriesChargeSensor>();
            if (component == null) return;
            activateValue = component.activateValue;
            deactivateValue = component.deactivateValue;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            powerCell = Grid.PosToCell(this);
            statusGUID = GetComponent<KSelectable>().AddStatusItem(StoredEnergyStatus, (object)this);
            this.OnToggle += this.OnSwitchToggled;
            this.UpdateVisualState(true);
        }

        private void OnSwitchToggled(bool toggled_on)
        {
            UpdateLogicCircuit();
            UpdateVisualState();
        }

        protected override void OnCleanUp()
        {
            GetComponent<KSelectable>().RemoveStatusItem(statusGUID);
        }

        public void Sim200ms(float dt)
        {
            UpdateSwitch();
        }

        private float GetBatteriesStoredEnergyPercent()
        {
            CircuitManager circuitManager = Game.Instance.circuitManager;
            if (circuitManager == null)
                return 0f;

            ushort circuitId = circuitManager.GetCircuitID(this.powerCell);
            List<Battery> batteries = circuitManager.GetBatteriesOnCircuit(circuitId);
            if (batteries == null || batteries.Count() == 0) return 0f;

            storedEnergy = 0;
            maxStoredEnergy = 0;
            foreach (Battery battery in batteries)
            {
                storedEnergy += battery.JoulesAvailable;
                maxStoredEnergy += battery.capacity;
            }
            return storedEnergy / maxStoredEnergy;
        }

        private void UpdateSwitch()
        {
            float percentFull = Mathf.RoundToInt(GetBatteriesStoredEnergyPercent() * 100f);
            if ((IsSwitchedOn && percentFull >= deactivateValue) || (!IsSwitchedOn && percentFull <= activateValue)) Toggle();
        }

        private void UpdateLogicCircuit()
        {

            logicPorts.SendSignal(BatteriesChargeSensor.PORT_ID, IsSwitchedOn ? 1 : 0);
        }

        private void UpdateVisualState(bool force = false)
        {
            if (this.wasOn == this.switchedOn & !force) return;
            this.wasOn = this.switchedOn;
            KBatchedAnimController component = this.GetComponent<KBatchedAnimController>();
            component.Play(this.switchedOn ? "on_pre" : "on_pst");
            component.Queue(this.switchedOn ? "on" : "off");
        }

        public float ActivateValue
        {
            get => (float)this.deactivateValue;
            set
            {
                this.deactivateValue = (int)value;
                this.UpdateLogicCircuit();
            }
        }

        public float DeactivateValue
        {
            get => (float)this.activateValue;
            set
            {
                this.activateValue = (int)value;
                this.UpdateLogicCircuit();
            }
        }

        public float MinValue => 0.0f;

        public float MaxValue => 100f;

        public bool UseWholeNumbers => true;

        public string ActivateTooltip => (string)BUILDINGS.PREFABS.BATTERYSMART.DEACTIVATE_TOOLTIP;

        public string DeactivateTooltip => (string)BUILDINGS.PREFABS.BATTERYSMART.ACTIVATE_TOOLTIP;

        public string ActivationRangeTitleText => (string)BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_TITLE;

        public string ActivateSliderLabelText => (string)BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_DEACTIVATE;

        public string DeactivateSliderLabelText => (string)BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_ACTIVATE;
    }
}
