namespace NetworkCommunication.Objects
{
    public enum SensorType : short
    {
        BloodPressure = 0x18,
        Respiration = 0x22,
        Temperature = 0x23, // Contributed by mamelmahdy: https://github.com/mindleaving/gedash/issues/2
        SpO2 = 0x2d,
        Ecg = 0x3a,
        
        
        EcgLeadI = 0x56,
        EcgLeadII = 0x57,
        EcgLeadIII = 0x58,
        EcgLeadPrecordial = 0x59,
        
        
        tCO2,

        Undefined,
        Raw
    }
}