using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CentralMonitorGUI.Views;
using Commons.Mathematics;
using Commons.Wpf;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class DataExplorerWindowViewModel
    {
        private readonly TimeSpan waveformTimeSpan = TimeSpan.FromSeconds(20);
        private readonly TimeSpan waveformDataExpansion = TimeSpan.FromSeconds(15);
        private readonly PatientInfo patientInfo;
        private readonly HistoryLoader historyLoader;
        private readonly AnnotationDatabase annotationDatabase;

        public DataExplorerWindowViewModel(
            PatientInfo patientInfo,
            HistoryLoader historyLoader, AnnotationDatabase annotationDatabase)
        {
            this.patientInfo = patientInfo;
            this.historyLoader = historyLoader;
            this.annotationDatabase = annotationDatabase;

            SelectedTime = new SelectedTime();
            AvailableDataPlotViewModel = new AvailableDataPlotViewModel(patientInfo, historyLoader);
            VitalSignPlotViewModel = new VitalSignPlotViewModel(SelectedTime);
            VitalSignPlotViewModel.PropertyChanged += VitalSignPlotViewModel_PropertyChanged;
            WaveformPlotViewModel = new HistoricWaveformPlotViewModel(SelectedTime);

            UpdateCommand = new RelayCommand(AvailableDataPlotViewModel.UpdateDataRange);
            LoadDataRangeCommand = new RelayCommand(LoadVitalSignDataForSelectedTimeRange);
            AnnotateCommand = new RelayCommand(AddAnnotation);

            AvailableDataPlotViewModel.UpdateDataRange();
        }

        private void VitalSignPlotViewModel_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if(propertyChangedEventArgs.PropertyName != nameof(VitalSignPlotViewModel.SelectedTime))
                return;
            if(VitalSignPlotViewModel.SelectedTime == DateTime.MinValue)
                return;
            LoadWaveformsForTime(VitalSignPlotViewModel.SelectedTime);
        }

        public SelectedTime SelectedTime { get; }

        public ICommand UpdateCommand { get; }
        public ICommand LoadDataRangeCommand { get; }
        public ICommand AnnotateCommand { get; }

        public AvailableDataPlotViewModel AvailableDataPlotViewModel { get; }
        public VitalSignPlotViewModel VitalSignPlotViewModel { get; }
        public HistoricWaveformPlotViewModel WaveformPlotViewModel { get; }

        private async void LoadVitalSignDataForSelectedTimeRange()
        {
            var timeRange = AvailableDataPlotViewModel.SelectedTimeRange;
            var sensorTypes = new[] {SensorType.SpO2, SensorType.Ecg, SensorType.BloodPressure, SensorType.Respiration }; // TODO
            var vitalSignTypes = new[]
            {
                VitalSignType.SpO2, VitalSignType.HeartRate, VitalSignType.RespirationRate,
                VitalSignType.SystolicBloodPressure, VitalSignType.DiastolicBloodPressure
            };
            var vitalSignData = await Task.Run(() => historyLoader.GetVitalSignDataInRange(patientInfo, timeRange, sensorTypes, vitalSignTypes));
            VitalSignPlotViewModel.PlotData(vitalSignData, timeRange);
            var annotations = annotationDatabase.Annotations.Where(annotation => timeRange.Contains(annotation.Timestamp));
            VitalSignPlotViewModel.SetAnnotations(annotations);
            WaveformPlotViewModel.ClearPlot();
        }

        private async void LoadWaveformsForTime(DateTime selectedTime)
        {
            WaveformPlotViewModel.ClearPlot();
            WaveformPlotViewModel.InstructionText = "Loading...";
            var rangeStart = selectedTime.Subtract(waveformDataExpansion);
            var rangeEnd = selectedTime.Add(waveformTimeSpan + waveformDataExpansion);
            var timeRange = new Range<DateTime>(rangeStart, rangeEnd);
            var sensorTypes = ((SensorType[]) Enum.GetValues(typeof(SensorType)))
                .Where(Informations.IsWaveformSensorType)
                .ToList();
            var waveformData = await Task.Run(() => historyLoader.GetWaveformDataInRange(patientInfo, timeRange, sensorTypes));
            var focusedRange = new Range<DateTime>(selectedTime, selectedTime + waveformTimeSpan);
            WaveformPlotViewModel.PlotWaveforms(waveformData, focusedRange);
            var annotations = annotationDatabase.Annotations.Where(annotation => timeRange.Contains(annotation.Timestamp));
            WaveformPlotViewModel.SetAnnotations(annotations);
        }

        private void AddAnnotation()
        {
            var annotationViewModel = new AnnotationNoteViewModel(SelectedTime.Time);
            var annotationWindow = new AnnotationNoteWindow {ViewModel = annotationViewModel};
            var result = annotationWindow.ShowDialog();
            if(result != true)
                return;
            annotationDatabase.Add(new Annotation(
                annotationViewModel.Timestamp,
                annotationViewModel.Title,
                annotationViewModel.Note));
        }
    }
}