using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Commons.Extensions;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class DataConnectionManager : IDisposable
    {
        private readonly MonitorNetwork network;
        private readonly DataRequestSender dataRequestSender;
        private readonly Dictionary<PatientMonitor, CancellationTokenSource> cancellationTokenSources = new Dictionary<PatientMonitor, CancellationTokenSource>();

        public DataConnectionManager(
            MonitorNetwork network,
            WaveformAndVitalSignReceiver waveformAndVitalSignReceiver,
            DataRequestSender dataRequestSender,
            AlarmReceiver alarmReceiver)
        {
            this.network = network;
            this.dataRequestSender = dataRequestSender;

            network.NewMonitorDiscovered += Network_NewMonitorDiscovered;
            network.MonitorDisappeared += Network_MonitorDisappeared;
            waveformAndVitalSignReceiver.NewVitalSignData += NewVitalSignsReceived;
            waveformAndVitalSignReceiver.NewWaveformData += NewWaveformDataReceived;

            alarmReceiver.NewAlarmReceived += AlarmReceiver_NewAlarmReceived;
        }

        private void Network_NewMonitorDiscovered(object sender, PatientMonitor newMonitor)
        {
            if(cancellationTokenSources.ContainsKey(newMonitor))
            {
                cancellationTokenSources[newMonitor].Cancel();
                cancellationTokenSources.Remove(newMonitor);
            }
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(newMonitor, cancellationTokenSource);
            var target = new IPEndPoint(newMonitor.IPAddress, Informations.DataRequestPort);
            dataRequestSender.StartRequesting( 
                target,
                TimeSpan.FromSeconds(5), 
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

        private void NewWaveformDataReceived(object sender, WaveformCollection waveformCollection)
        {
            if (!network.Monitors.ContainsKey(waveformCollection.IPAddress))
                return;
            var monitor = network.Monitors[waveformCollection.IPAddress];
            monitor.AddWaveformData(waveformCollection);
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
