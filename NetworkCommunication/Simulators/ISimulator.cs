using System.Collections.Generic;

namespace NetworkCommunication.Simulators
{
    public interface ISimulator
    {
        byte QualityByte { get; }
        SensorType SensorType { get; }
        IList<VitalSignValue> GetVitalSignValues();
        short GetNextValue();
    }
}