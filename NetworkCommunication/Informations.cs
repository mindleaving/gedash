using System;
using System.Collections.Generic;
using NetworkCommunication.Objects;

namespace NetworkCommunication
{
    public static class Informations
    {
        public const int SensorBatchesPerSecond = 60;

        public static readonly Dictionary<SensorType, int> SensorBatchSizes = new Dictionary<SensorType, int>
        {
            { SensorType.EcgLeadI, 4},
            { SensorType.EcgLeadII, 4},
            { SensorType.EcgLeadIII, 4},
            { SensorType.EcgLeadPrecordial, 4},
            { SensorType.RespirationRate, 1 },
            { SensorType.SpO2, 1},
            { SensorType.Other, 1},
            { SensorType.Raw, 1}
        };

        public static readonly Dictionary<SensorType, short> SensorWaveformCode = new Dictionary<SensorType, short>
        {
            { SensorType.EcgLeadI, 1803},
            { SensorType.EcgLeadII, 2059},
            { SensorType.EcgLeadIII, 2315},
            { SensorType.EcgLeadPrecordial, 2571},
            { SensorType.RespirationRate, 5897 },
            { SensorType.SpO2, 9993}
        };

        public static readonly Dictionary<SensorType, uint> SensorVitalSignCode = new Dictionary<SensorType, uint>
        {
            { SensorType.Ecg, 0x00013ac0},
            { SensorType.EcgLeadI, 0x000156c0},
            { SensorType.EcgLeadII, 0x000157c0},
            { SensorType.EcgLeadIII, 0x000158c0},
            { SensorType.EcgLeadPrecordial, 0x000159c0},
            { SensorType.RespirationRate, 0x000122c0 },
            { SensorType.SpO2, 0x00012dc0},
            { SensorType.BloodPressure, 0x000118c0 }
        };

        public static SensorType MapWaveformCodeToSensorType(short code)
        {
            if (code >= 1800 && code < 2050)
                return SensorType.EcgLeadI;
            if (code >= 2050 && code < 2300)
                return SensorType.EcgLeadII;
            if (code >= 2300 && code < 2570)
                return SensorType.EcgLeadIII;
            if (code >= 2570 && code < 2850)
                return SensorType.EcgLeadPrecordial;
            if (code == 5897)
                return SensorType.RespirationRate;
            if (code == 9993)
                return SensorType.SpO2;
            throw new ArgumentOutOfRangeException();
        }

        public static SensorType MapVitalSignSenorCodeToSensorType(uint code)
        {
            var filter = 0xfffffff0;
            var filteredCode = code & filter;
            if (filteredCode == 0x00013ac0)
                return SensorType.Ecg;
            if (filteredCode == 0x000122c0)
                return SensorType.RespirationRate;
            if (filteredCode == 0x00012dc0)
                return SensorType.SpO2;
            if (filteredCode == 0x000118c0)
                return SensorType.BloodPressure;
            if (filteredCode == 0x000156c0)
                return SensorType.EcgLeadI;
            if (filteredCode == 0x000157c0)
                return SensorType.EcgLeadII;
            if (filteredCode == 0x000158c0)
                return SensorType.EcgLeadIII;
            if (filteredCode == 0x000159c0)
                return SensorType.EcgLeadPrecordial;
            throw new ArgumentOutOfRangeException();
        }

        public static IList<VitalSignType> VitalSignTypesForSensor(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return new List<VitalSignType>
                    {
                        VitalSignType.HeartRate,
                        VitalSignType.VentricularExtraSystoles,
                        VitalSignType.UnknownEcgParameter
                    };
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    return new List<VitalSignType>();
                case SensorType.RespirationRate:
                    return new List<VitalSignType> { VitalSignType.RespirationRate };
                case SensorType.BloodPressure:
                    return new List<VitalSignType>
                    {
                        VitalSignType.DiastolicBloodPressure, 
                        VitalSignType.SystolicBloodPressure, 
                        VitalSignType.MeanArterialPressure
                    };
                case SensorType.SpO2:
                    return new[] {VitalSignType.SpO2, VitalSignType.HeartRate};
                default:
                    return new List<VitalSignType>();
            }
        }

        public static TimeSpan SensorTypeSampleTime(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Ecg:
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    return TimeSpan.FromSeconds(1.0/240);
                case SensorType.RespirationRate:
                case SensorType.SpO2:
                    return TimeSpan.FromSeconds(1.0/60);
                default:
                    return TimeSpan.Zero;
            }
        }

        public const int DiscoveryMessageSourcePort = 3123;
        public const int DiscoveryTargetPort = 7000;
        public const int VitalSignsRequestOutboundPort = 3073;
        public const int WaveformRequestOutboundPort = 3074;
        public const int DataRequestPort = 2000;
        public const int WaveformSourcePort = 3075;
        public const int VitalSignSourcePort = 3076;
    }
}
