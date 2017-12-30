namespace NetworkCommunication.Objects
{
    public class Alarm
    {
        public Alarm(
            PatientMonitor monitor, 
            SensorType sensorType, 
            VitalSignValue triggeringValue)
        {
            Monitor = monitor;
            SensorType = sensorType;
            TriggeringValue = triggeringValue;
        }

        public PatientMonitor Monitor { get; }
        public SensorType SensorType { get; }
        public VitalSignValue TriggeringValue { get; }
    }
}