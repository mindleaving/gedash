using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public interface IVitalSignSource
    {
        SensorType SensorType { get; }
        void Update(VitalSignValue vitalSignValue);
        IReadOnlyCollection<VitalSignValue> VitalSignValues { get; }
    }
}