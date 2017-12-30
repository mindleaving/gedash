using System;
using System.Collections.Generic;
using System.Net;
using NetworkCommunication.DataStorage;

namespace NetworkCommunication.Objects
{
    public class PatientMonitor
    {
        private static readonly TimeSpan waveformBufferTime = TimeSpan.FromSeconds(30);
        private readonly List<VitalSignValue> vitalSignValues = new List<VitalSignValue>();

        public PatientMonitor(
            IPAddress ipAddress, 
            string wardName, 
            string bedName, 
            PatientInfo patientInfo)
        {
            IPAddress = ipAddress;
            WardName = wardName;
            BedName = bedName;
            PatientInfo = patientInfo;
        }

        public IPAddress IPAddress { get; }
        public string WardName { get; }
        public string BedName { get; }
        public PatientInfo PatientInfo { get; }
        public DateTime LastContactTime { get; set; }

        public Dictionary<SensorType, IWaveformSource> WaveformSources { get; } = new Dictionary<SensorType, IWaveformSource>();
        public IReadOnlyCollection<VitalSignValue> VitalSignValues => vitalSignValues;
        public event EventHandler<SensorType> NewWaveformSensorConnected;

        public void UpdateVitalSign(VitalSignValue vitalSignValue)
        {
            vitalSignValues.RemoveAll(x =>
                x.SensorType == vitalSignValue.SensorType
                && x.VitalSignType == vitalSignValue.VitalSignType);
            vitalSignValues.Add(vitalSignValue);
        }

        public void AddWaveformData(WaveformData waveformData)
        {
            foreach (var sensorType in waveformData.SensorWaveforms.Keys)
            {
                if(!Informations.IsWaveformSensorType(sensorType))
                    continue;
                var waveformValues = waveformData.SensorWaveforms[sensorType];
                if(!WaveformSources.ContainsKey(sensorType))
                {
                    var samplesPerSecond = Informations.SensorBatchesPerSecond * Informations.SensorBatchSizes[sensorType];
                    var bufferSize = (int)(waveformBufferTime.TotalSeconds * samplesPerSecond);
                    WaveformSources.Add(sensorType, new WaveformBuffer(sensorType, bufferSize));
                    NewWaveformSensorConnected?.Invoke(this, sensorType);
                }
                WaveformSources[sensorType].AddData(waveformValues);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PatientMonitor)obj);
        }

        private bool Equals(PatientMonitor other)
        {
            return Equals(IPAddress, other.IPAddress);
        }

        public override int GetHashCode()
        {
            return (IPAddress != null ? IPAddress.GetHashCode() : 0);
        }
    }
}
