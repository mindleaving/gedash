using LiveCharts;
using LiveCharts.Wpf;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class WaveformViewModel
    {
        public WaveformViewModel(SensorType sensorType, 
            IDataSource waveformSource,
            UpdateTrigger updateTrigger)
        {
            SensorType = sensorType;
            WaveformSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ChartPoint>()
                }
            };
        }

        public SensorType SensorType { get; }
        public SeriesCollection WaveformSeries { get; }
    }
}