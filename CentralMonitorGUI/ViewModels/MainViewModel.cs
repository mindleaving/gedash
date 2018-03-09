using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CentralMonitorGUI.Views;
using Commons.Wpf;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TimeSpan timeToShow = TimeSpan.FromSeconds(10);
        private readonly UpdateTrigger updateTrigger;
        private readonly FileManager fileManager;
        private readonly DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory;
        private readonly Action closeWindow;

        public MainViewModel(
            MonitorNetwork network,
            UpdateTrigger updateTrigger,
            FileManager fileManager,
            DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory,
            Action closeWindow)
        {
            this.updateTrigger = updateTrigger;
            this.fileManager = fileManager;
            this.dataExplorerWindowViewModelFactory = dataExplorerWindowViewModelFactory;
            this.closeWindow = closeWindow;

            ExitCommand = new RelayCommand(closeWindow);
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

        public ICommand ExitCommand { get; }
        public ICommand OpenPatientDatabaseCommand { get; }

        private void OpenPatientDatabase()
        {
            OpenDataExplorerWindow();
        }

        private Thread databaseWindowThread;
        private readonly object dataExplorerCreationLock = new object();
        private void OpenDataExplorerWindow()
        {
            if (databaseWindowThread != null
                && databaseWindowThread.IsAlive)
            {
                //dataExplorerWindow?.Activate();
                return;
            }
            lock (dataExplorerCreationLock)
            {
                if (databaseWindowThread != null
                    && databaseWindowThread.IsAlive)
                {
                    //dataExplorerWindow?.Activate();
                    return;
                }
                databaseWindowThread?.Abort();
                databaseWindowThread?.Join();
                databaseWindowThread = new Thread(CreateDatabaseWindow);
                databaseWindowThread.SetApartmentState(ApartmentState.STA);
                databaseWindowThread.IsBackground = true;
                databaseWindowThread.Start();
                while (!databaseWindowThread.IsAlive)
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void CreateDatabaseWindow()
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            var patientDatabaseViewModel = new PatientDatabaseViewModel(fileManager, dataExplorerWindowViewModelFactory);
            var patientDatabaseWindow = new PatientDatabaseWindow { ViewModel = patientDatabaseViewModel };
            patientDatabaseWindow.Closed += (sender, args) =>
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            patientDatabaseWindow.Show();
            Dispatcher.Run();
        }
    }
}
