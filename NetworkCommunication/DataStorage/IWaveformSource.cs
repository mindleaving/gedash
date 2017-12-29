using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public interface IWaveformSource
    {
        SensorType SensorType { get; }
        void AddData(WaveformData data);
        IEnumerable<short> GetValues(int valueCount);
    }
}
