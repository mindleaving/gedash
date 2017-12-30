using System;
using System.Collections.Generic;
using System.Threading;
using Commons;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class DataConnectionManager : IDisposable
    {
        private readonly MonitorNetwork network;
        private readonly WaveformAndVitalSignReceiver waveformAndVitalSignReceiver;
        private readonly AlarmReceiver alarmReceiver;
        private readonly Dictionary<PatientMonitor, CancellationTokenSource> cancellationTokenSources = new Dictionary<PatientMonitor, CancellationTokenSource>();

        public DataConnectionManager(
            MonitorNetwork network,
            WaveformAndVitalSignReceiver waveformAndVitalSignReceiver,
            AlarmReceiver alarmReceiver)
        {
            this.network = network;
            this.waveformAndVitalSignReceiver = waveformAndVitalSignReceiver;
            this.alarmReceiver = alarmReceiver;

            network.NewMonitorDiscovered += Network_NewMonitorDiscovered;
            network.MonitorDisappeared += Network_MonitorDisappeared;
            waveformAndVitalSignReceiver.NewVitalSignData += NewVitalSignsReceived;
            waveformAndVitalSignReceiver.NewWaveformData += NewWaveformDataReceived;

            alarmReceiver.NewAlarmReceived += AlarmReceiver_NewAlarmReceived;
        }

        private void Network_NewMonitorDiscovered(object sender, PatientMonitor newMonitor)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(newMonitor, cancellationTokenSource);
            waveformAndVitalSignReceiver.StartReceiving(
                newMonitor.IPAddress,
                cancellationTokenSource.Token);
        }

        private void Network_MonitorDisappeared(object sender, PatientMonitor disappearedMonitor)
        {
            if(!cancellationTokenSources.ContainsKey(disappearedMonitor))
                return;
            cancellationTokenSources[disappearedMonitor].Cancel();
        }

        private void NewVitalSignsReceived(object sender, VitalSignData vitalSignData)
        {
            if(!network.Monitors.ContainsKey(vitalSignData.IPAddress))
                return;
            var monitor = network.Monitors[vitalSignData.IPAddress];
            monitor.VitalSignValues = vitalSignData.VitalSignValues;
        }

        private void NewWaveformDataReceived(object sender, WaveformData waveformData)
        {
            if (!network.Monitors.ContainsKey(waveformData.IPAddress))
                return;
            var monitor = network.Monitors[waveformData.IPAddress];
            monitor.AddWaveformData(waveformData);
        }

        private void AlarmReceiver_NewAlarmReceived(object sender, Alarm alarm)
        {
            if(!network.Monitors.ContainsKey(alarm.IPAddress))
                return;
            var monitor = network.Monitors[alarm.IPAddress];
            monitor.AddAlarm(alarm);
        }

        public void Dispose()
        {
            cancellationTokenSources.Values.ForEach(x => x.Cancel());
        }
    }
}
