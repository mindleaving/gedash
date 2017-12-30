using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public interface IWaveformSource
    {
        SensorType SensorType { get; }
        int AvailableSampleCount { get; }
        void AddData(IList<short> data);
        IEnumerable<short> GetValues(int valueCount);
    }
}
