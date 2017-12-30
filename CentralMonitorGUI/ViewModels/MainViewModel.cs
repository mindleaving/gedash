using System;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkCommunication;
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

            ChartViewModel1 = new ChartCanvasViewModel(1000);
            ChartViewModel2 = new ChartCanvasViewModel(1000);
            updateTrigger.Trig += UpdateTrigger_Trig;
        }

        private int valueIdx;
        private string testText;

        private void UpdateTrigger_Trig(object sender, EventArgs e)
        {
            ChartViewModel1.Values[valueIdx].Value = StaticRng.RNG.Next(0, 100);
            ChartViewModel2.Values[valueIdx].Value = StaticRng.RNG.Next(0, 100);
            valueIdx++;
            if (valueIdx == ChartViewModel1.Values.Count)
                valueIdx = 0;
            //ChartViewModel1.Values[valueIdx].Value = double.NaN;
            //ChartViewModel2.Values[valueIdx].Value = double.NaN;
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

        public ChartCanvasViewModel ChartViewModel1 { get; }
        public ChartCanvasViewModel ChartViewModel2 { get; }
    }
}
