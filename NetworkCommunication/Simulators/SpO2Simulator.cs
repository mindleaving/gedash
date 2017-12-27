using System.Collections.Generic;

namespace NetworkCommunication.Simulators
{
    public class SpO2Simulator : ISimulator
    {
        readonly SimulationSettings settings;
        int valueIdx;

        static readonly IList<short> spO2Values = new short[]
        {
            664, 664, 736, 824, 1168, 1168, 1376, 1376, 1472, 1472, 1376, 1376, 1128, 1128, 976, 976, 856, 856, 848,
            848, 888, 888, 904, 904, 872, 872, 808, 808, 712, 712, 664, 664, 616, 616, 608, 608, 600, 600, 592, 592,
            592, 592, 592, 592, 592, 592, 584, 584, 568, 568, 552, 552, 528, 528, 528, 528, 584, 584
        };

        public SpO2Simulator(SimulationSettings settings)
        {
            this.settings = settings;
        }

        public byte QualityByte { get; } = 0xe0;
        public SensorType SensorType { get; } = SensorType.SpO2;
        public short SpO2 { get; set; } = 99;
        public short SpO2LowerLimit { get; set; } = 90;
        public short SpO2UpperLimit { get; set; } = 105;
        public short HeartRate { get; set; } = 60;
        public short HeartRateLowerLimit { get; set; } = 50;
        public short HeartRateUpperLimit { get; set; } = 100;

        public IList<VitalSignValue> GetVitalSignValues()
        {
            var spO2 = SpO2 + StaticRng.RNG.Next(-1, 2);
            if (spO2 > 100)
                spO2 = 100;
            if (spO2 < 92)
                spO2 = 92;
            SpO2 = (short) spO2;
            var heartRate = HeartRate + StaticRng.RNG.Next(-1, 2);
            if (heartRate > 80)
                heartRate = 80;
            if (heartRate < 50)
                heartRate = 50;
            HeartRate = (short) heartRate;
            return new List<VitalSignValue>
            {
                new VitalSignValue(SensorType, VitalSignType.SpO2, SpO2, SpO2LowerLimit, SpO2UpperLimit),
                new VitalSignValue(SensorType, VitalSignType.HeartRate, HeartRate, HeartRateLowerLimit, HeartRateUpperLimit)
            };
        }

        public short GetNextValue()
        {
            valueIdx++;
            if (valueIdx >= spO2Values.Count)
                valueIdx = 0;
            return spO2Values[valueIdx];
        }
    }
}
