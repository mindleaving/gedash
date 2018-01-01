using System;
using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Simulators
{
    public class EcgSimulator : ISimulator
    {
        private readonly SimulationSettings settings;
        private int valueIdx;

        private static readonly IList<short> ecgValues = new short[]
        {
            1, 15, 29, 43, 56, 69, 79, 84, 87, 89, 87, 78, 62, 37, 8, -22, -50, -77, -101, -126, -148, -167, -183, -199,
            -214, -227, -239, -251, -270, -269, -271, -273, -270, -267, -268, -269, -269, -266, -264, -263, -265, -267,
            -266, -264, -264, -263, -261, -259, -261, -263, -265, -264, -262, -260, -260, -260, -262, -264, -264, -264,
            -264, -264, -269, -275, -279, -281, -281, -281, -281, -280, -279, -279, -282, -287, -290, -292, -293, -292,
            -289, -287, -288, -290, -293, -294, -293, -292, -293, -292, -288, -286, -286, -284, -282, -285, -290, -295,
            -294, -292, -290, -291, -292, -292, -291, -290, -292, -291, -287, -286, -288, -291, -293, -293, -291, -291,
            -291, -288, -282, -281, -285, -288, -287, -286, -287, -287, -287, -286, -284, -284, -284, -280, -273, -266,
            -257, -246, -236, -227, -219, -209, -196, -187, -188, -197, -235, -244, -257, -278, -299, -307, -306, -302,
            -299, -299, -300, -302, -304, -304, -301, -298, -296, -293, -291, -289, -291, -291, -292, -292, -289, -288,
            -293, -296, -286, -258, -225, -174, -82, 50, 188, 292, 278, 116, -99, -276, -387, -427, -426, -414, -397,
            -365, -324, -285, -257, -238, -226, -220, -216, -212, -211, -212, -212, -199, -196, -191, -188, -188, -186,
            -183, -176, -169, -163, -162, -161, -160, -154, -147, -140, -134, -127, -119, -110, -101, -88, -75, -66,
            -57, -44, -29, -14, 1
        };

        public EcgSimulator(
            SensorType sensorType, 
            SimulationSettings settings)
        {
            if(!sensorType.InSet(SensorType.EcgLeadI, SensorType.EcgLeadII, SensorType.EcgLeadIII, SensorType.EcgLeadPrecordial))
                throw new ArgumentException("Sensor type must be of an ECG type");
            this.settings = settings;
            SensorType = sensorType;
            Lead = Informations.MapSensorTypeToLead(sensorType);
        }

        public byte QualityByte { get; } = 0x08; // 0x08 = Valid, 0x1f = lead failure
        public SensorType SensorType { get; }
        public EcgLead Lead { get; }
        public short Heartrate { get; set; } = 61;
        public short HeartrateLowerLimit { get; set; } = 50;
        public short HeartRateUpperLimit { get; set; } = 100;
        public short VentricularExtraSystoles { get; set; } = 3;
        public short VentricularExtraSystolesLowerLimit { get; set; } = 0;
        public short VentricularExtraSystolesUpperLimit { get; set; } = 6;

        public IList<VitalSignValue> GetVitalSignValues()
        {
            return new List<VitalSignValue>
            {
                new VitalSignValue(SensorType, VitalSignType.HeartRate,
                    Heartrate, HeartrateLowerLimit, HeartRateUpperLimit),
                new VitalSignValue(SensorType, VitalSignType.VentricularExtraSystoles,
                    VentricularExtraSystoles, VentricularExtraSystolesLowerLimit, VentricularExtraSystolesUpperLimit),
                new VitalSignValue(SensorType, VitalSignType.UnknownEcgParameter,
                    0, 0, 0)
            };
        }

        public short GetNextValue()
        {
            valueIdx++;
            if (valueIdx >= ecgValues.Count)
                valueIdx = 0;
            return (short)(ecgValues[valueIdx]+270);
        }
    }
}
