﻿using System;
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

namespace CentralMonitorGUI.ViewModels
{
    public class HistoricWaveformPlotViewModel : ViewModelBase
    {
        private readonly SelectedTime selectedTime;
        private readonly int seriesSeparation = 0;
        private readonly Axis xAxis;
        private readonly Axis yAxis;
        private string instructionText = "Hold SHIFT and click on plot above for showing waveforms";
        private DataPoint measuringStartPoint = DataPoint.Undefined;
        private DataPoint measuringEndPoint = DataPoint.Undefined;
        private Range<DateTime> focusedTimeRange;

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
                    var y = yOffset + timePoint.Value - sensorMaxValue;
                    series.Points.Add(new DataPoint(x, y));
                }
                PlotModel.Series.Add(series);
                series.MouseDown += Series_MouseDown;

                yOffset -= sensorYSpan;
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

            PlotModel.Annotations.Clear();
            if (measuringStartPoint.IsDefined())
            {
                PlotModel.Annotations.Add(new LineAnnotation
                {
                    X = measuringStartPoint.X,
                    Type = LineAnnotationType.Vertical,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 2,
                });
            }
            if (measuringEndPoint.IsDefined())
            {
                PlotModel.Annotations.Add(new LineAnnotation
                {
                    X = measuringEndPoint.X,
                    Type = LineAnnotationType.Vertical,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 2,
                });
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
            selectedTime.Source = AnnotationType.Waveforms;
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
