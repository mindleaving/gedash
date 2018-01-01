using System;
using System.Collections.Generic;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class VitalSignFileLoader
    {
        public Dictionary<SensorVitalSignType, TimeSeries<short>> Load(
            string directory, 
            Range<DateTime> timeRange, 
            IReadOnlyList<SensorType> sensorTypes, 
            IReadOnlyList<VitalSignType> vitalSignTypes)
        {
            throw new NotImplementedException();
        }
    }
}