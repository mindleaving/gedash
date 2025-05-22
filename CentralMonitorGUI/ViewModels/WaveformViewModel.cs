using System;
using System.Linq;
using System.Windows.Media;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CentralMonitorGUI.ViewModels
{
    public class WaveformViewModel : ViewModelBase, IDisposable
    {
        private readonly IWaveformSource waveformSource;
        private readonly UpdateTrigger updateTrigger;
        private readonly int sampleCountPerUpdate;
        private int sampleIdx;

        public WaveformViewModel(
            SensorType sensorType, 
            IWaveformSource waveformSource,
            UpdateTrigger updateTrigger,
            TimeSpan timeToShow)
        {
            this.waveformSource = waveformSource;
            this.updateTrigger = updateTrigger;
            SensorType = sensorType;
            TimeToShow = timeToShow;
            sampleCountPerUpdate = Informations.SensorBatchSizes[sensorType];
            SetupPlotModel();
            updateTrigger.Trig += UpdateTrigger_Trig;
        }

        private static Color MapSensorTypeToBrush(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Ecg:
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    return Colors.LawnGreen;
                case SensorType.Respiration:
                    return Colors.Yellow;
                case SensorType.SpO2:
                    return Colors.LightSkyBlue;
                default:
                    return Colors.White;
            }
        }

        private void SetupPlotModel()
        {
            PlotModel = new PlotModel
            {
                Axes = { new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    IsAxisVisible = false,
                    IsZoomEnabled = false,
                    IsPanEnabled = false
                }}
            };

            var sensorColor = MapSensorTypeToBrush(SensorType);
            PlotModel.Series.Add(new LineSeries
            {
                Color = OxyColor.FromArgb(sensorColor.A, sensorColor.R, sensorColor.G, sensorColor.B),
                MarkerType = MarkerType.None,
                LineStyle = LineStyle.Solid
            });
        }

        private volatile bool isUpdating;
        private void UpdateTrigger_Trig(object? sender, EventArgs e)
        {
            if(isUpdating)
                return;
            var samplesPerSecond = Informations.SensorBatchesPerSecond * Informations.SensorBatchSizes[SensorType];
            var sampleCount = (int) (TimeToShow.TotalSeconds * samplesPerSecond);
            lock (PlotModel.SyncRoot)
            {
                isUpdating = true;

                var newSamples = waveformSource.GetValues(waveformSource.AvailableSampleCount - sampleCountPerUpdate).ToList();
                var series = (LineSeries)PlotModel.Series[0];
                foreach (var sample in newSamples)
                {
                    series.Points.Add(new DataPoint(sampleIdx, sample));
                    sampleIdx++;
                }
                while (series.Points.Count > sampleCount)
                    series.Points.RemoveAt(0);

                isUpdating = false;
            }
            PlotModel.InvalidatePlot(true);
        }

        public SensorType SensorType { get; }
        public TimeSpan TimeToShow { get; set; }
        public PlotModel PlotModel { get; private set; }

        public void Dispose()
        {
            updateTrigger.Trig -= UpdateTrigger_Trig;
        }
    }
}