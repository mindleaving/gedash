using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformBuffer : IWaveformSource
    {

        public WaveformBuffer(SensorType sensorType, int bufferSize)
        {
            SensorType = sensorType;
        }

        public SensorType SensorType { get; }

        public void AddData(WaveformData data)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<short> GetValues(int valueCount)
        {
            throw new System.NotImplementedException();
        }
    }
}
