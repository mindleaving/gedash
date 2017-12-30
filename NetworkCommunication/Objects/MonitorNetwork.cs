using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NetworkCommunication.Communicators;

namespace NetworkCommunication.Objects
{
    public class MonitorNetwork
    {
        private readonly TimeSpan connectionLostTimeout;
        private readonly Dictionary<IPAddress, PatientMonitor> monitors = new Dictionary<IPAddress, PatientMonitor>();
        private readonly Dictionary<PatientMonitor, Timer> timeoutTimers = new Dictionary<PatientMonitor, Timer>();

        public MonitorNetwork(
            DiscoveryMessageReceiver discoveryMessageReceiver,
            TimeSpan connectionLostTimeout)
        {
            this.connectionLostTimeout = connectionLostTimeout;
            discoveryMessageReceiver.DiscoveryMessageReceived += NewDiscoveryMessageReceived;
        }

        public IReadOnlyDictionary<IPAddress, PatientMonitor> Monitors => monitors;
        public event EventHandler<PatientMonitor> NewMonitorDiscovered;
        public event EventHandler<PatientMonitor> MonitorDisappeared;

        private void NewDiscoveryMessageReceived(object sender, DiscoveryMessage discoveryMessage)
        {
            var now = DateTime.UtcNow;
            if (Monitors.ContainsKey(discoveryMessage.IPAddress))
            {
                var existingMonitor = Monitors[discoveryMessage.IPAddress];
                existingMonitor.LastContactTime = now;
                timeoutTimers[existingMonitor].Change(connectionLostTimeout, TimeSpan.FromMilliseconds(-1));
                return;
            }

            var newMonitor = new PatientMonitor(
                discoveryMessage.IPAddress,
                discoveryMessage.WardName,
                discoveryMessage.BedName,
                discoveryMessage.PatientInfo)
            {
                LastContactTime = now
            };
            monitors.Add(newMonitor.IPAddress, newMonitor);
            NewMonitorDiscovered?.Invoke(this, newMonitor);
            var connectionTimeoutTimer = new Timer(
                ConnectionTimeoutOccurred,
                newMonitor, 
                connectionLostTimeout, 
                TimeSpan.FromMilliseconds(-1));
            timeoutTimers.Add(newMonitor, connectionTimeoutTimer);
        }

        private void ConnectionTimeoutOccurred(object state)
        {
            var monitor = (PatientMonitor) state;
            MonitorDisappeared?.Invoke(this, monitor);
            monitors.Remove(monitor.IPAddress);
            var timer = timeoutTimers[monitor];
            timeoutTimers.Remove(monitor);
            timer.Dispose();
        }
    }
}
