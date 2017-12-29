namespace NetworkCommunication.Objects
{
    public enum SensorType : short
    {
        Ecg,
        EcgLeadI,
        EcgLeadII,
        EcgLeadIII,
        EcgLeadPrecordial,
        RespirationRate,
        SpO2,
        BloodPressure,

        Other,
        Raw
    }
}