using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CentralMonitorGUI.Views;
using Commons.Wpf;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;
using Timer = System.Timers.Timer;

namespace CentralMonitorGUI.ViewModels
{
    public class PatientMonitorViewModel : ViewModelBase
    {
        private bool ecgEnabled = true;
        private bool respirationEnabled = true;
        private bool spO2Enabled = true;
        private EcgLead selectedEcgLead = EcgLead.II;
        private readonly UpdateTrigger updateTrigger;
        private readonly TimeSpan timeToShow;
        private readonly DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory;
        private readonly Dictionary<SensorType, WaveformViewModel> waveformViewModels = new Dictionary<SensorType, WaveformViewModel>();
        private VitalSignViewModel vitalSignValues;
        private WaveformViewModel ecgWaveform;
        private WaveformViewModel respirationWaveform;
        private WaveformViewModel spO2Waveform;
        private readonly Timer alarmTimeoutTimer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds) { AutoReset = false };
        private Brush borderBrush = Brushes.Transparent;
        private Brush alarmTextBrush = Brushes.Transparent;
        private bool isSelected;
        private Alarm activeAlarm;
        private Brush infoBarBackground = Brushes.LightGoldenrodYellow;
        private readonly object dataExplorerCreationLock = new object();
        private Thread dataExplorerThread;

        public PatientMonitorViewModel(
            PatientMonitor monitor, 
            UpdateTrigger updateTrigger, 
            TimeSpan timeToShow,
            DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory)
        {
            Monitor = monitor;
            this.updateTrigger = updateTrigger;
            this.timeToShow = timeToShow;
            this.dataExplorerWindowViewModelFactory = dataExplorerWindowViewModelFactory;

            VitalSignValues = new VitalSignViewModel(monitor);
            OpenDataExplorerWindowCommand = new RelayCommand(OpenDataExplorerWindow);
            EcgWaveform = new WaveformViewModel(SensorType.EcgLeadII, new WaveformBuffer(SensorType.EcgLeadII, 0), updateTrigger, timeToShow);
            RespirationWaveform = new WaveformViewModel(SensorType.Respiration, new WaveformBuffer(SensorType.Respiration, 0), updateTrigger, timeToShow);
            SpO2Waveform = new WaveformViewModel(SensorType.SpO2, new WaveformBuffer(SensorType.SpO2, 0), updateTrigger, timeToShow);
            alarmTimeoutTimer.Elapsed += AlarmTimeoutTimer_Elapsed;
            monitor.NewWaveformSensorConnected += Monitor_NewWaveformSensorConnected;
            monitor.NewAlarm += Monitor_NewAlarm;
            foreach (var kvp in monitor.WaveformSources)
            {
                var sensorType = kvp.Key;
                var waveformSource = kvp.Value;
                var waveformViewModel = new WaveformViewModel(
                    sensorType,
                    waveformSource,
                    updateTrigger,
                    timeToShow);
                waveformViewModels.Add(sensorType, waveformViewModel);
                SetWaveformViewModel(sensorType, waveformViewModel);
            }
        }

        private void AlarmTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BorderBrush = Brushes.Transparent;
            ActiveAlarm = null;
            AlarmTextBrush = Brushes.Transparent;
            InfoBarBackground = Brushes.LightGoldenrodYellow;
        }

        private void Monitor_NewAlarm(object sender, Alarm alarm)
        {
            alarmTimeoutTimer.Stop();
            ActiveAlarm = alarm;
            AlarmTextBrush = Brushes.Red;
            BorderBrush = Brushes.Red;
            InfoBarBackground = Brushes.MistyRose;
            alarmTimeoutTimer.Start();
        }

        private void Monitor_NewWaveformSensorConnected(object sender, SensorType sensorType)
        {
            if(waveformViewModels.ContainsKey(sensorType))
                return;
            var waveformSource = Monitor.WaveformSources[sensorType];
            var waveformViewModel = new WaveformViewModel(
                sensorType,
                waveformSource,
                updateTrigger,
                timeToShow);
            waveformViewModels[sensorType] = waveformViewModel;
            SetWaveformViewModel(sensorType, waveformViewModel);
        }

        private void SetWaveformViewModel(SensorType sensorType, WaveformViewModel waveformViewModel)
        {
            switch (sensorType)
            {
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    var lead = Informations.MapSensorTypeToLead(sensorType);
                    if (SelectedEcgLead == lead)
                        EcgWaveform = waveformViewModel;
                    break;
                case SensorType.Respiration:
                    RespirationWaveform = waveformViewModel;
                    break;
                case SensorType.SpO2:
                    SpO2Waveform = waveformViewModel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null);
            }
        }

        public PatientMonitor Monitor { get; }


        public Alarm ActiveAlarm
        {
            get { return activeAlarm; }
            set
            {
                activeAlarm = value; 
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value; 
                OnPropertyChanged();
            }
        }

        public Brush AlarmTextBrush
        {
            get
            {
                return alarmTextBrush;
            }
            private set
            {
                alarmTextBrush = value;
                OnPropertyChanged();
            }
        }
        public Brush BorderBrush
        {
            get
            {
                return IsSelected
                    ? Brushes.Blue
                    : borderBrush;
            }
            private set
            {
                borderBrush = value; 
                OnPropertyChanged();
            }
        }

        public ICommand OpenDataExplorerWindowCommand { get; }

        public WaveformViewModel EcgWaveform
        {
            get { return ecgWaveform; }
            set
            {
                ecgWaveform = value;
                OnPropertyChanged();
            }
        }

        public WaveformViewModel RespirationWaveform
        {
            get { return respirationWaveform; }
            set
            {
                respirationWaveform = value;
                OnPropertyChanged();
            }
        }

        public WaveformViewModel SpO2Waveform
        {
            get { return spO2Waveform; }
            set
            {
                spO2Waveform = value;
                OnPropertyChanged();
            }
        }

        public VitalSignViewModel VitalSignValues
        {
            get { return vitalSignValues; }
            set
            {
                vitalSignValues = value; 
                OnPropertyChanged();
            }
        }

        public bool EcgEnabled
        {
            get { return ecgEnabled; }
            set
            {
                ecgEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool RespirationEnabled
        {
            get { return respirationEnabled; }
            set
            {
                respirationEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool SpO2Enabled
        {
            get { return spO2Enabled; }
            set
            {
                spO2Enabled = value;
                OnPropertyChanged();
            }
        }

        public IList<EcgLead> EcgLeads { get; } = (EcgLead[]) Enum.GetValues(typeof(EcgLead));
        public EcgLead SelectedEcgLead
        {
            get { return selectedEcgLead; }
            set
            {
                SensorType ecgSensorType;
                try
                {
                    ecgSensorType = Informations.MapLeadToSensorType(value);
                    selectedEcgLead = value;
                }
                catch (Exception)
                {
                    MessageBox.Show($"Lead {value} is not available");
                    ecgSensorType = SensorType.EcgLeadII;
                    selectedEcgLead = EcgLead.II;
                }
                if(waveformViewModels.ContainsKey(ecgSensorType))
                {
                    EcgWaveform = waveformViewModels[ecgSensorType];
                }
                else
                {
                    EcgWaveform = new WaveformViewModel(
                        ecgSensorType,
                        new WaveformBuffer(ecgSensorType, 0),
                        updateTrigger,
                        timeToShow);
                }
                OnPropertyChanged();
            }
        }

        public Brush InfoBarBackground
        {
            get { return infoBarBackground; }
            set
            {
                infoBarBackground = value; 
                OnPropertyChanged();
            }
        }

        private void OpenDataExplorerWindow()
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
                dataExplorerThread = new Thread(() => CreateDataExplorerWindow(Monitor.PatientInfo));
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
            var dataExplorerWindow = new DataExplorerWindow { ViewModel = dataExplorerViewModel };
            dataExplorerWindow.Closed += (sender, args) =>
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            dataExplorerWindow.Show();
            Dispatcher.Run();
        }
    }
}