using System;
using System.Collections.Generic;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformFileLoader
    {
        public Dictionary<SensorType, TimeSeries<short>> Load(
            string directory, 
            Range<DateTime> timeRange, 
            IReadOnlyList<SensorType> sensorTypes)
        {
            throw new NotImplementedException();
        }
    }
}