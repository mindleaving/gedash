using System.Collections.Generic;

namespace NetworkCommunication.Simulators
{
    public class NibpSimualtor : ISimulator
    {
        readonly SimulationSettings settings;

        public NibpSimualtor(SimulationSettings settings)
        {
            this.settings = settings;
        }

        public byte QualityByte { get; } = 0x00;
        public short DiastolicPressure { get; set; } = 80;
        public short DiastolicPressureLowerLimit { get; set; } = 50;
        public short DiastolicPressureUpperLimit { get; set; } = 100;
        public short SystolicPressure { get; set; } = 120;
        public short SystolicPressureLowerLimit { get; set; } = 60;
        public short SystolicPressureUpperLimit { get; set; } = 140;
        public short MeanArterialPressure { get; set; } = 102;
        public short MeanArterialPressureLowerLimit { get; set; } = 90;
        public short MeanArterialPressureUpperLimit { get; set; } = 110;
        public SensorType SensorType { get; } = SensorType.BloodPressure;

        public IList<VitalSignValue> GetVitalSignValues()
        {
            return new List<VitalSignValue>
            {
                new VitalSignValue(SensorType, VitalSignType.DiastolicBloodPressure, 
                    DiastolicPressure, DiastolicPressureLowerLimit, DiastolicPressureUpperLimit),
                new VitalSignValue(SensorType, VitalSignType.SystolicBloodPressure,
                    SystolicPressure, SystolicPressureLowerLimit, SystolicPressureUpperLimit),
                new VitalSignValue(SensorType, VitalSignType.MeanArterialPressure,
                    MeanArterialPressure, MeanArterialPressureLowerLimit, MeanArterialPressureUpperLimit)
            };
        }

        public short GetNextValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
