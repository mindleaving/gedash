using System;
using System.Collections.Generic;
using System.Net;
using NetworkCommunication.DataStorage;

namespace NetworkCommunication.Objects
{
    public class PatientMonitor
    {
        private static readonly TimeSpan WaveformBufferTime = TimeSpan.FromSeconds(30);
        private List<VitalSignValue> vitalSignValues = new List<VitalSignValue>();

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
        public Alarm LatestAlarm { get; private set; }
        public event EventHandler<Alarm> NewAlarm;

        public Dictionary<SensorType, IWaveformSource> WaveformSources { get; } = new Dictionary<SensorType, IWaveformSource>();
        public event EventHandler<SensorType> NewWaveformSensorConnected;

        public List<VitalSignValue> VitalSignValues
        {
            get { return vitalSignValues; }
            set { vitalSignValues = value ?? new List<VitalSignValue>(); }
        }

        public void AddWaveformData(WaveformCollection waveformCollection)
        {
            foreach (var sensorType in waveformCollection.SensorWaveforms.Keys)
            {
                if(!sensorType.IsWaveformSensorType())
                    continue;
                var waveformValues = waveformCollection.SensorWaveforms[sensorType];
                if(!WaveformSources.ContainsKey(sensorType))
                {
                    var samplesPerSecond = Informations.SensorBatchesPerSecond * Informations.SensorBatchSizes[sensorType];
                    var bufferSize = (int)(WaveformBufferTime.TotalSeconds * samplesPerSecond);
                    WaveformSources.Add(sensorType, new WaveformBuffer(sensorType, bufferSize));
                    NewWaveformSensorConnected?.Invoke(this, sensorType);
                }
                WaveformSources[sensorType].AddData(waveformValues);
            }
        }

        public void AddAlarm(Alarm alarm)
        {
            LatestAlarm = alarm;
            NewAlarm?.Invoke(this, alarm);
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
