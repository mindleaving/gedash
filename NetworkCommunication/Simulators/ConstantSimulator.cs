using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Simulators
{
    public class ConstantSimulator : ISimulator
    {
        private readonly short value;
        private readonly VitalSignType vitalSignType;

        public ConstantSimulator(
            SensorType sensorType, 
            VitalSignType vitalSignType, 
            short value)
        {
            SensorType = sensorType;
            this.vitalSignType = vitalSignType;
            this.value = value;
        }

        public byte QualityByte { get; } = 0x00;
        public SensorType SensorType { get; }

        public IList<VitalSignValue> GetVitalSignValues()
        {
            return new List<VitalSignValue>
            {
                new VitalSignValue(SensorType, vitalSignType, value, value, value)
            };
        }

        public short GetNextValue()
        {
            return value;
        }
    }
}
