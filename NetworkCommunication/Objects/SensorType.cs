namespace NetworkCommunication.Objects
{
    public enum SensorType : short
    {
        Ecg,
        EcgLeadI,
        EcgLeadII,
        EcgLeadIII,
        EcgLeadPrecordial,
        Respiration,
        SpO2,
        BloodPressure,

        Undefined,
        Raw
    }
}