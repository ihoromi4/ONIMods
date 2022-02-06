using System;
using System.Collections.Generic;
using UnityEngine;

namespace PassiveGasVentInput
{
    class PassiveGasVentInput : StateMachineComponent<PassiveGasVentInput.SMInstance>, ISim1000ms, IGameObjectEffectDescriptor
    {
        [MyCmpReq] private Operational operational;
        [MyCmpGet] private KSelectable selectable;
        [MyCmpReq] private ElementConsumer consumer;
        [MyCmpGet] private ConduitDispenser dispenser;
        [MyCmpReq] private KBatchedAnimController animController;
        private Guid conduitBlockedStatusGuid;
        private Guid noElementStatusGuid;
        private float CONSUMPTION_PER_PRESSURE_KG;
        private PassiveGasVentInputSettings settings;

        protected override void OnSpawn()
        {
            CONSUMPTION_PER_PRESSURE_KG = (PassiveGasVentInputSettings.Instance.MaximumFlow - PassiveGasVentInputSettings.Instance.MinimumFlow) / (PassiveGasVentInputSettings.Instance.MaximumPressure - PassiveGasVentInputSettings.Instance.MinimumPressure);
            settings = PassiveGasVentInputSettings.Instance;
            smi.StartSM();
            this.dispenser.GetConduitManager().AddConduitUpdater(this.OnConduitUpdate, ConduitFlowPriority.LastPostUpdate);
        }

        protected override void OnCleanUp()
        {
            this.dispenser.GetConduitManager().RemoveConduitUpdater(this.OnConduitUpdate);
            base.OnCleanUp();
        }

        private void OnConduitUpdate(float dt)
        {
            this.conduitBlockedStatusGuid = this.selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlocked, conduitBlockedStatusGuid, dispenser.blocked);
        }

        public bool IsLogicOff { get { return !operational.GetFlag(LogicOperationalController.LogicOperationalFlag); } }

        public bool IsButtonOff { get { return !operational.GetFlag(BuildingEnabledButton.EnabledFlag); } }

        public int Cell { get { return Grid.PosToCell(transform.GetPosition()); } }

        public float GasPressure { get { return Grid.Mass[Cell]; } }

        public bool IsGasInCell { get { return Grid.Element[Cell].IsState(Element.State.Gas); } }

        public bool IsEnoughMass { get { return Grid.Mass[Cell] >= PassiveGasVentInputSettings.Instance.MinimumPressure; } }

        public void Sim1000ms(float dt)
        {
            noElementStatusGuid = this.selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoGasElementToPump, this.noElementStatusGuid, !IsGasInCell);
        }

        public void UpdateConsumption(float dt)
        {
            UpdateConsumptionRate();
            SetAnimSpeed(0.75f + 0.75f * (consumer.consumptionRate / PassiveGasVentInputSettings.Instance.MaximumFlow));
        }

        private void UpdateConsumptionRate()
        {
            if (IsGasInCell & IsEnoughMass)
            {
                // TODO: find another solution
                // when stored gass mass > consumer.capacityKG consumer must be deactivated
                // but consumer.capacityKG stored gas mass limitation doesn't work
                // probably it's becouse consumer.elementToConsume didn't set
                // and consumer cannot find out storage gas mass
                // temporary we use the fix external to Consumer class
                if (consumer.storage.MassStored() >= consumer.capacityKG)
                {
                    consumer.consumptionRate = 0f;
                }
                else
                {
                    float consumptionRate = settings.MinimumFlow + (GasPressure - settings.MinimumPressure) * CONSUMPTION_PER_PRESSURE_KG;
                    consumer.consumptionRate = Mathf.Clamp(Mathf.Round(consumptionRate * 1000f) / 1000f, settings.MinimumFlow, settings.MaximumFlow);
                }
                consumer.RefreshConsumptionRate();
            }
        }

        public void SetAnimSpeed(float factor)
        {
            animController.PlaySpeedMultiplier = factor;
            float positionPercent = this.animController.GetPositionPercent();
            this.animController.Play(this.animController.CurrentAnim.hash, this.animController.PlayMode);
            this.animController.SetPositionPercent(positionPercent);
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            List<Descriptor> descriptorList = new List<Descriptor>();

            Descriptor descriptor = new Descriptor();
            descriptor.SetupDescriptor(string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVEGASVENTINPUT.DESCRIPTORS.REQ_PRESSURE_DESC, GameUtil.GetFormattedMass(PassiveGasVentInputSettings.Instance.MinimumPressure)), string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVEGASVENTINPUT.DESCRIPTORS.REQ_PRESSURE_TOOLTIP, GameUtil.GetFormattedMass(PassiveGasVentInputSettings.Instance.MinimumPressure)), Descriptor.DescriptorType.Effect);
            descriptorList.Add(descriptor);

            descriptor = new Descriptor();
            float consumptionPerKg = (PassiveGasVentInputSettings.Instance.MaximumFlow - PassiveGasVentInputSettings.Instance.MinimumFlow) / (PassiveGasVentInputSettings.Instance.MaximumPressure - PassiveGasVentInputSettings.Instance.MinimumPressure);
            descriptor.SetupDescriptor(STRINGS.BUILDINGS.PREFABS.PASSIVEGASVENTINPUT.DESCRIPTORS.DEP_PRESSURE_DESC, string.Format(STRINGS.BUILDINGS.PREFABS.PASSIVEGASVENTINPUT.DESCRIPTORS.DEP_PRESSURE_TOOLTIP, GameUtil.GetFormattedMass(consumptionPerKg)), Descriptor.DescriptorType.Effect);
            descriptorList.Add(descriptor);

            return descriptorList;
        }

        public class SMInstance : GameStateMachine<States, SMInstance, PassiveGasVentInput, object>.GameInstance
        {
            public SMInstance(PassiveGasVentInput master) : base(master)
            {
            }

            public bool IsOperational => master.operational.IsOperational;

            public bool IsEnoughGasMass => master.IsGasInCell && master.IsEnoughMass;

            public bool IsClosed => master.IsLogicOff | master.IsButtonOff;

            public bool IsOpenedIdle => !master.IsLogicOff & !master.IsButtonOff & !IsOperational;

            public void Update(float dt)
            {
                master.UpdateConsumption(dt);
            }
        }

        public class States : GameStateMachine<States, SMInstance, PassiveGasVentInput>
        {
            public State Closed;
            public State Idle;
            public State PumpingPre;
            public State Pumping;
            public State PumpingPst;

            public override void InitializeStates(out BaseState defaultState)
            {
                defaultState = Idle;

                Closed
                    .QueueAnim("off")
                    .EventTransition(GameHashes.OperationalFlagChanged, Idle, smi => smi.IsOpenedIdle)
                    .EventTransition(GameHashes.OperationalChanged, PumpingPre, smi => smi.IsOperational & smi.IsEnoughGasMass);

                Idle
                    .QueueAnim("on")
                    .Update("Idle", (smi, dt) => { if (smi.IsEnoughGasMass && smi.IsOperational) smi.GoTo(Pumping); }, UpdateRate.SIM_200ms)
                    .EventTransition(GameHashes.OperationalFlagChanged, Closed, smi => smi.IsClosed);

                PumpingPre
                    .QueueAnim("working_pre")
                    .OnAnimQueueComplete(Pumping);

                Pumping
                    .Enter(smi => smi.master.operational.SetActive(true))
                    .Exit(smi => { smi.master.operational.SetActive(false); smi.master.SetAnimSpeed(1f); })
                    .QueueAnim("working_loop", true)
                    .Update("Pumping", (smi, dt) => { if (smi.IsEnoughGasMass) smi.Update(dt); else smi.GoTo(Idle); }, UpdateRate.SIM_1000ms)
                    .EventTransition(GameHashes.OperationalChanged, Idle, smi => smi.IsOpenedIdle)
                    .EventTransition(GameHashes.OperationalChanged, PumpingPst, smi => smi.IsClosed);

                PumpingPst
                    .QueueAnim("working_pst")
                    .OnAnimQueueComplete(Closed);
            }
        }
    }
}
