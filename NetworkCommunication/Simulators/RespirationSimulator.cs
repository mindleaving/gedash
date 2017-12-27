using System.Collections.Generic;

namespace NetworkCommunication.Simulators
{
    public class RespirationSimulator : ISimulator
    {
        readonly SimulationSettings settings;
        int valueIdx;

        static readonly IList<short> respirationValues = new short[]
        {
            -251, -259, -268, -277, -286, -294, -302, -310, -318, -326, -333, -341, -349, -357, -365, -372, -379, -386,
            -391, -396, -400, -402, -404, -405, -405, -404, -401, -398, -393, -388, -381, -374, -366, -358, -349, -339,
            -330, -321, -313, -177, -297, -290, -283, -275, -268, -261, -253, -245, -236, -228, -218, -209, -200, -190,
            -181, -172, -163, -153, -144, -135, -126, -117, -108, -99, -90, -81, -72, -63, -55, -46, -38, -30, -22, -14,
            -6, 1, 9, 17, 24, 32, 40, 48, 55, 63, 71, 78, 86, 94, 101, 108, 115, 121, 128, 134, 140, 146, 151, 156, 160,
            163, 165, 167, 169, 171, 173, 175, 177, 180, 182, 186, 189, 194, 198, 203, 207, 212, 216, 220, 225, 229,
            234, 238, 242, 247, 251, 256, 260, 264, 267, 271, 275, 278, 281, 284, 287, 289, 291, 293, 295, 297, 298,
            299, 300, 300, 301, 301, 301, 301, 301, 301, 301, 300, 300, 299, 298, 296, 295, 293, 291, 288, 285, 281,
            277, 272, 267, 261, 256, 250, 245, 240, 235, 231, 227, 223, 220, 217, 213, 82, 207, 203, 199, 195, 191, 187,
            183, 179, 175, 170, 165, 161, 156, 151, 146, 141, 135, 130, 124, 119, 113, 107, 102, 96, 90, 84, 79, 73, 67,
            61, 55, 50, 45, 39, 34, 28, 23, 18, 13, 8, 3, -1, -6, -11, -16, -22, -28, -34, -41, -48, -56, -64, -72, -80,
            -88, -96, -103, -110, -117, -123, -129, -135, -141, -147, -153, -158, -164, -170, -177, -183, -190, -197,
            -204, -210, -217, -224, -231, -238, -244, -251
        };

        public RespirationSimulator(SimulationSettings settings)
        {
            this.settings = settings;
        }

        public byte QualityByte { get; } = 0x00;
        public short RespirationRate { get; set; } = 11;
        public short RespirationRateLowerLimit { get; set; } = 5;
        public short RespirationRateUpperLimit { get; set; } = 30;
        public SensorType SensorType { get; } = SensorType.RespirationRate;

        public IList<VitalSignValue> GetVitalSignValues()
        {
            var respirationRate = RespirationRate + StaticRng.RNG.Next(-1, 2);
            if (respirationRate > 25)
                respirationRate = 25;
            else if (respirationRate < 8)
                respirationRate = 8;
            RespirationRate = (short) respirationRate;
            return new List<VitalSignValue>
            {
                new VitalSignValue(SensorType, VitalSignType.RespirationRate, RespirationRate, RespirationRateLowerLimit, RespirationRateUpperLimit)
            };
        }

        public short GetNextValue()
        {
            valueIdx++;
            if (valueIdx >= respirationValues.Count)
                valueIdx = 0;
            return respirationValues[valueIdx];
        }
    }
}
