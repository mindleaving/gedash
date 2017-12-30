using System;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkCommunication.Communicators;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TimeSpan timeToShow = TimeSpan.FromSeconds(3);
        private readonly MonitorNetwork network;
        private readonly DataConnectionManager dataConnectionManager;
        private readonly UpdateTrigger updateTrigger;

        public MainViewModel(
            MonitorNetwork network,
            DataConnectionManager dataConnectionManager,
            UpdateTrigger updateTrigger)
        {
            this.network = network;
            this.dataConnectionManager = dataConnectionManager;
            this.updateTrigger = updateTrigger;

            network.NewMonitorDiscovered += Network_NewMonitorDiscovered;
            network.MonitorDisappeared += Network_MonitorDisappeared;
        }

        private void Network_NewMonitorDiscovered(object sender, PatientMonitor newMonitor)
        {
            var viewModel = new PatientMonitorViewModel(
                newMonitor,
                updateTrigger, 
                timeToShow);
            Monitors.Add(viewModel);
        }

        private void Network_MonitorDisappeared(object sender, PatientMonitor disappearedMonitor)
        {
            var viewModelToBeRemoved = Monitors.SingleOrDefault(
                monitor => monitor.Monitor.Equals(disappearedMonitor));
            if(viewModelToBeRemoved == null)
                return;
            Monitors.Remove(viewModelToBeRemoved);
        }

        public ObservableCollection<PatientMonitorViewModel> Monitors { get; } = new ObservableCollection<PatientMonitorViewModel>();
    }
}
