namespace NetworkCommunication.Objects
{
    public class VitalSignValue
    {
        public VitalSignValue(SensorType sensorType, 
            VitalSignType vitalSignType,
            short value,
            short lowerLimit,
            short upperLimit)
        {
            SensorType = sensorType;
            VitalSignType = vitalSignType;
            Value = value;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }

        public SensorType SensorType { get; }
        public VitalSignType VitalSignType { get; }
        public short Value { get; }
        public short LowerLimit { get; }
        public short UpperLimit { get; }
    }
}
