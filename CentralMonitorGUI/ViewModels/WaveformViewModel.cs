using System;
using System.Linq;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class WaveformViewModel : ViewModelBase, IDisposable
    {
        private readonly IWaveformSource waveformSource;
        private readonly TimeSpan timeToShow;
        private readonly UpdateTrigger updateTrigger;
        private readonly int sampleCountPerUpdate;
        private int currentSampleIdx;
        private readonly ChartValues<ObservableValue> chartValues;

        public WaveformViewModel(
            SensorType sensorType, 
            IWaveformSource waveformSource,
            UpdateTrigger updateTrigger,
            TimeSpan timeToShow)
        {
            this.waveformSource = waveformSource;
            this.timeToShow = timeToShow;
            this.updateTrigger = updateTrigger;
            SensorType = sensorType;
            sampleCountPerUpdate = Informations.SensorBatchSizes[sensorType];
            var samplesPerSecond = Informations.SensorBatchesPerSecond * Informations.SensorBatchSizes[sensorType];
            var sampleCount = (int) (timeToShow.TotalSeconds * samplesPerSecond);
            var initialValues = Enumerable.Range(0, sampleCount)
                .Select(idx => new ObservableValue(double.NaN));
            chartValues = new ChartValues<ObservableValue>(initialValues);
            WaveformSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = chartValues
                }
            };
            updateTrigger.Trig += UpdateTrigger_Trig;
        }

        private void UpdateTrigger_Trig(object sender, EventArgs e)
        {
            var newSamples = waveformSource.GetValues(sampleCountPerUpdate).ToList();
            for (int sampleIdx = 0; sampleIdx < sampleCountPerUpdate; sampleIdx++)
            {
                if (newSamples.Count > sampleIdx)
                    chartValues[currentSampleIdx].Value = newSamples[sampleIdx];
                else
                    chartValues[currentSampleIdx].Value = double.NaN;
                currentSampleIdx++;
                if (currentSampleIdx == chartValues.Count)
                    currentSampleIdx = 0;
            }
            // Invalid next value
            chartValues[currentSampleIdx].Value = double.NaN;
        }

        public SensorType SensorType { get; }
        public SeriesCollection WaveformSeries { get; }

        public void Dispose()
        {
            updateTrigger.Trig -= UpdateTrigger_Trig;
        }
    }
}