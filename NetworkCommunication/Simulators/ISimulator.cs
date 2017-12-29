using System.Collections.Generic;
using NetworkCommunication.Objects;

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