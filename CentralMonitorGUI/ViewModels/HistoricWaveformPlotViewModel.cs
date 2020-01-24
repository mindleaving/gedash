using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using NetworkCommunication.Objects;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Annotation = NetworkCommunication.Objects.Annotation;

namespace CentralMonitorGUI.ViewModels
{
    public class HistoricWaveformPlotViewModel : ViewModelBase
    {
        private readonly SelectedTime selectedTime;
        private readonly int seriesSeparation = 100;
        private readonly Axis xAxis;
        private readonly Axis yAxis;
        private string instructionText = "Hold SHIFT and click on plot above for showing waveforms";
        private DataPoint measuringStartPoint = DataPoint.Undefined;
        private DataPoint measuringEndPoint = DataPoint.Undefined;
        private Range<DateTime> focusedTimeRange;
        private OxyPlot.Annotations.Annotation line1;
        private OxyPlot.Annotations.Annotation line2;

        public HistoricWaveformPlotViewModel(SelectedTime selectedTime)
        {
            this.selectedTime = selectedTime;
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

        private string measurementText = "";
        public string MeasurementText
        {
            get { return measurementText; }
            set
            {
                measurementText = value;
                OnPropertyChanged();
            }
        }

        public string MeasurementInstructionText { get; } = "Hold SHIFT for selecting start position and CTRL to select end position";

        public void PlotWaveforms(
            IReadOnlyDictionary<SensorType, TimeSeries<short>> waveforms, 
            Range<DateTime> focusedTimeRange)
        {
            this.focusedTimeRange = focusedTimeRange;
            PlotModel.Series.Clear();
            PlotModel.Annotations.Clear();
            line1 = null;
            line2 = null;

            var yOffset = 0.0;
            foreach (var kvp in waveforms.OrderBy(kvp => GetSensorOrder(kvp.Key)))
            {
                var sensorType = kvp.Key;
                var timeSeries = kvp.Value;
                if(!timeSeries.Any())
                    continue;

                var sensorMinMaxMean = new MinMaxMean(timeSeries.Select(x => (double)x.Value));
                var sensorYSpan = Math.Min(1400, sensorMinMaxMean.Span);
                yOffset -= 0.5 * sensorYSpan; // Move down by half the Y-span

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
                    var y = yOffset + timePoint.Value - sensorMinMaxMean.Mean;
                    series.Points.Add(new DataPoint(x, y));
                }
                PlotModel.Series.Add(series);
                series.MouseDown += Series_MouseDown;

                yOffset -= 0.5*sensorYSpan;  // Move down by second half of the Y-span
                yOffset -= seriesSeparation;
            }
            xAxis.Zoom(0, (focusedTimeRange.To - focusedTimeRange.From).TotalSeconds);
            PlotModel.InvalidatePlot(true);
            InstructionText = "";
        }

        private void Series_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if(!(sender is LineSeries))
                return;
            if(!e.IsShiftDown && !e.IsControlDown)
                return;
            var series = (LineSeries) sender;
            var plotPosition = series.InverseTransform(e.Position);
            if (e.IsShiftDown)
                measuringStartPoint = plotPosition;
            else if (e.IsControlDown)
                measuringEndPoint = plotPosition;
            SetSelectedTime(focusedTimeRange.From.AddSeconds(plotPosition.X));

            if (measuringStartPoint.IsDefined())
            {
                if(line1 != null)
                    PlotModel.Annotations.Remove(line1);
                line1 = new LineAnnotation
                {
                    X = measuringStartPoint.X,
                    Type = LineAnnotationType.Vertical,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 2,
                };
                PlotModel.Annotations.Add(line1);
            }
            if (measuringEndPoint.IsDefined())
            {
                if(line2 != null)
                    PlotModel.Annotations.Remove(line2);
                line2 = new LineAnnotation
                {
                    X = measuringEndPoint.X,
                    Type = LineAnnotationType.Vertical,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 2,
                };
                PlotModel.Annotations.Add(line2);
            }
            PlotModel.InvalidatePlot(false);

            if (measuringStartPoint.IsDefined() && measuringEndPoint.IsDefined())
            {
                var deltaX = (measuringEndPoint.X - measuringStartPoint.X).Abs();
                var deltaY = (measuringEndPoint.Y - measuringStartPoint.Y).Abs();
                var frequencyPerMinute = 60 / deltaX;
                MeasurementText = $"Time: {deltaX:F3} s,   Y: {deltaY:F0},   Frequency: {frequencyPerMinute:F0} per minute";
            }
        }

        private void SetSelectedTime(DateTime timestamp)
        {
            selectedTime.Time = timestamp;
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
                case SensorType.Temperature:
                    return Colors.Green;
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
                case SensorType.Temperature:
                    return 2;
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

        public void SetAnnotations(IEnumerable<Annotation> annotations)
        {
            PlotModel.Annotations.Clear();
            if(line1 != null)
                PlotModel.Annotations.Add(line1);
            if(line2 != null)
                PlotModel.Annotations.Add(line2);
            if (focusedTimeRange != null)
                annotations = annotations.Where(annotation => focusedTimeRange.Contains(annotation.Timestamp));
            annotations.ForEach(annotation => PlotModel.Annotations.Add(new LineAnnotation
            {
                FontSize = 10,
                X = (annotation.Timestamp - focusedTimeRange.From).TotalSeconds,
                Text = annotation.Title,
                ToolTip = annotation.Note,
                Type = LineAnnotationType.Vertical,
                Color = OxyColor.FromRgb(Colors.Purple.R, Colors.Purple.G, Colors.Purple.B),
                StrokeThickness = 4
            }));
            PlotModel.InvalidatePlot(false);
        }
    }
}
