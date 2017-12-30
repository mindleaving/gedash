using System.Collections.Generic;
using Commons;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformBuffer : IWaveformSource
    {
        private readonly ConcurrentCappedQueue<short> buffer;

        public WaveformBuffer(SensorType sensorType, int bufferSize)
        {
            SensorType = sensorType;
            buffer = new ConcurrentCappedQueue<short>(bufferSize);
        }

        public SensorType SensorType { get; }
        public int AvailableSampleCount => buffer.Count;

        public void AddData(IList<short> data)
        {
            data.ForEach(x => buffer.Enqueue(x));
        }

        public IEnumerable<short> GetValues(int valueCount)
        {
            var valueIdx = 0;
            while (valueIdx < valueCount && buffer.TryDequeue(out var value))
            {
                yield return value;
                valueIdx++;
            }
        }
    }
}
