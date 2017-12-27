﻿using System.Collections.Generic;

namespace NetworkCommunication
{
    public class Waveform
    {
        public SensorType SensorType { get; }
        public List<short> Values { get; }

        public Waveform(SensorType sensorType, List<short> values)
        {
            SensorType = sensorType;
            Values = values;
        }
    }
}