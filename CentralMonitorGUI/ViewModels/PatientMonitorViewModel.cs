using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

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
        private readonly Dictionary<SensorType, WaveformViewModel> waveformViewModels = new Dictionary<SensorType, WaveformViewModel>();
        private VitalSignViewModel vitalSignValues;
        private WaveformViewModel ecgWaveform;
        private WaveformViewModel respirationWaveform;
        private WaveformViewModel spO2Waveform;

        public PatientMonitorViewModel(
            PatientMonitor monitor, 
            UpdateTrigger updateTrigger, 
            TimeSpan timeToShow)
        {
            Monitor = monitor;
            this.updateTrigger = updateTrigger;
            this.timeToShow = timeToShow;

            VitalSignValues = new VitalSignViewModel(monitor);
            EcgWaveform = new WaveformViewModel(SensorType.EcgLeadII, new WaveformBuffer(SensorType.EcgLeadII, 0), updateTrigger, timeToShow);
            RespirationWaveform = new WaveformViewModel(SensorType.Respiration, new WaveformBuffer(SensorType.Respiration, 0), updateTrigger, timeToShow);
            SpO2Waveform = new WaveformViewModel(SensorType.SpO2, new WaveformBuffer(SensorType.SpO2, 0), updateTrigger, timeToShow);
            monitor.NewWaveformSensorConnected += Monitor_NewWaveformSensorConnected;
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

        private void Monitor_NewWaveformSensorConnected(object sender, SensorType sensorType)
        {
            if(!waveformViewModels.ContainsKey(sensorType))
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
                    EcgWaveform = waveformViewModels[ecgSensorType];
                EcgWaveform = new WaveformViewModel(
                    ecgSensorType,
                    new WaveformBuffer(ecgSensorType, 0),
                    updateTrigger,
                    timeToShow);
                OnPropertyChanged();
            }
        }
    }
}