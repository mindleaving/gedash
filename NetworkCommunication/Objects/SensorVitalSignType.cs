using System;

namespace NetworkCommunication.Objects
{
    public struct SensorVitalSignType : IEquatable<SensorVitalSignType>
    {
        public SensorType SensorType { get; }
        public VitalSignType VitalSignType { get; }

        public SensorVitalSignType(SensorType sensorType, VitalSignType vitalSignType)
        {
            SensorType = sensorType;
            VitalSignType = vitalSignType;
        }

        public bool Equals(SensorVitalSignType other)
        {
            return SensorType == other.SensorType 
                   && VitalSignType == other.VitalSignType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SensorVitalSignType && Equals((SensorVitalSignType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) SensorType * 397) ^ (int) VitalSignType;
            }
        }
    }
}
