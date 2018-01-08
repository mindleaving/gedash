using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Commons.Mathematics;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CentralMonitorGUI.ViewModels
{
    public class HistoricWaveformPlotViewModel : ViewModelBase
    {
        private readonly int seriesSeparation = 0;
        private readonly Axis xAxis;
        private readonly Axis yAxis;
        private string instructionText = "Hold SHIFT and click on plot above for showing waveforms";

        public HistoricWaveformPlotViewModel()
        {
            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom
            };
            yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };
            PlotModel = new PlotModel
            {
                LegendBackground = OxyColor.FromRgb(255,255,255),
                LegendBorder = OxyColor.FromRgb(0,0,0),
                LegendBorderThickness = 2,
                LegendPosition = LegendPosition.TopRight
            };
            PlotModel.Axes.Add(xAxis);
            PlotModel.Axes.Add(yAxis);
        }

        public PlotModel PlotModel { get; }

        public string InstructionText
        {
            get { return instructionText; }
            set
            {
                instructionText = value; 
                OnPropertyChanged();
            }
        }

        public void PlotWaveforms(
            IReadOnlyDictionary<SensorType, TimeSeries<short>> waveforms, 
            Range<DateTime> focusedTimeRange)
        {
            PlotModel.Series.Clear();

            var yOffset = 0;
            foreach (var kvp in waveforms.OrderBy(kvp => GetSensorOrder(kvp.Key)))
            {
                var sensorType = kvp.Key;
                var timeSeries = kvp.Value;
                if(!timeSeries.Any())
                    continue;

                var sensorMinValue = timeSeries.Min(x => x.Value);
                var sensorMaxValue = timeSeries.Max(x => x.Value);
                var sensorYSpan = sensorMaxValue - sensorMinValue;

                var sensorColor = GetSensorColor(sensorType);
                var series = new LineSeries
                {
                    Title = sensorType.ToString(),
                    Color = OxyColor.FromRgb(sensorColor.R, sensorColor.G, sensorColor.B),
                    LineLegendPosition = LineLegendPosition.End
                };
                foreach (var timePoint in timeSeries)
                {
                    var x = (timePoint.Time - focusedTimeRange.From).TotalSeconds;
                    var y = yOffset + timePoint.Value - sensorMinValue;
                    series.Points.Add(new DataPoint(x, y));
                }
                PlotModel.Series.Add(series);

                yOffset += sensorYSpan;
                yOffset += seriesSeparation;
            }
            xAxis.Zoom(0, (focusedTimeRange.To - focusedTimeRange.From).TotalSeconds);
            PlotModel.InvalidatePlot(true);
            InstructionText = "";
        }

        private Color GetSensorColor(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    return Colors.Black;
                case SensorType.Respiration:
                    return Colors.DarkOrange;
                case SensorType.SpO2:
                    return Colors.DodgerBlue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null);
            }
        }

        public void ClearPlot()
        {
            PlotModel.Series.Clear();
            PlotModel.InvalidatePlot(true);
            InstructionText = "Hold SHIFT and click on plot above for showing waveforms";
        }

        private static int GetSensorOrder(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.SpO2:
                    return 0;
                case SensorType.Respiration:
                    return 1;
                case SensorType.EcgLeadI:
                    return 10;
                case SensorType.EcgLeadII:
                    return 11;
                case SensorType.EcgLeadIII:
                    return 12;
                case SensorType.EcgLeadPrecordial:
                    return 13;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null);
            }
        }
    }
}
