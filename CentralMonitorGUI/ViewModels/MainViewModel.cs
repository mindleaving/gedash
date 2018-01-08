using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CentralMonitorGUI.Views;
using Commons.Wpf;
using NetworkCommunication.Communicators;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TimeSpan timeToShow = TimeSpan.FromSeconds(10);
        private readonly MonitorNetwork network;
        private readonly DataConnectionManager dataConnectionManager;
        private readonly UpdateTrigger updateTrigger;
        private readonly DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory;

        public MainViewModel(
            MonitorNetwork network,
            DataConnectionManager dataConnectionManager,
            UpdateTrigger updateTrigger,
            DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory)
        {
            this.network = network;
            this.dataConnectionManager = dataConnectionManager;
            this.updateTrigger = updateTrigger;
            this.dataExplorerWindowViewModelFactory = dataExplorerWindowViewModelFactory;

            OpenPatientDatabaseCommand = new RelayCommand(OpenPatientDatabase);

            network.NewMonitorDiscovered += Network_NewMonitorDiscovered;
            network.MonitorDisappeared += Network_MonitorDisappeared;
        }

        private void Network_NewMonitorDiscovered(object sender, PatientMonitor newMonitor)
        {
            var viewModel = new PatientMonitorViewModel(
                newMonitor,
                updateTrigger, 
                timeToShow,
                dataExplorerWindowViewModelFactory);
            Monitors.Add(viewModel);
        }

        private void Network_MonitorDisappeared(object sender, PatientMonitor disappearedMonitor)
        {
            var viewModelToBeRemoved = Monitors.SingleOrDefault(
                monitor => monitor.Monitor.Equals(disappearedMonitor));
            if(viewModelToBeRemoved == null)
                return;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => Monitors.Remove(viewModelToBeRemoved)));
        }

        public ObservableCollection<PatientMonitorViewModel> Monitors { get; } = new ObservableCollection<PatientMonitorViewModel>();

        public ICommand OpenPatientDatabaseCommand { get; }

        private void OpenPatientDatabase()
        {
            OpenDataExplorerWindow(new PatientInfo("J", "SCHOLTYSSEK"));
        }

        private Thread dataExplorerThread;
        private readonly object dataExplorerCreationLock = new object();
        private void OpenDataExplorerWindow(PatientInfo patientInfo)
        {
            if (dataExplorerThread != null
                && dataExplorerThread.IsAlive)
            {
                //dataExplorerWindow?.Activate();
                return;
            }
            lock (dataExplorerCreationLock)
            {
                if (dataExplorerThread != null
                    && dataExplorerThread.IsAlive)
                {
                    //dataExplorerWindow?.Activate();
                    return;
                }
                dataExplorerThread?.Abort();
                dataExplorerThread?.Join();
                dataExplorerThread = new Thread(() => CreateDataExplorerWindow(patientInfo));
                dataExplorerThread.SetApartmentState(ApartmentState.STA);
                dataExplorerThread.IsBackground = true;
                dataExplorerThread.Start();
                while (!dataExplorerThread.IsAlive)
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void CreateDataExplorerWindow(PatientInfo patientInfo)
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            var dataExplorerViewModel = dataExplorerWindowViewModelFactory.Create(patientInfo);
            var dataExplorerWindow = new DataExplorerWindow(dataExplorerViewModel);
            dataExplorerWindow.Closed += (sender, args) =>
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            dataExplorerWindow.Show();
            Dispatcher.Run();
        }
    }
}
