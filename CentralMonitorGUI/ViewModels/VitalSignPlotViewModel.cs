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
using OxyPlot.Legends;
using OxyPlot.Series;
using Annotation = NetworkCommunication.Objects.Annotation;

namespace CentralMonitorGUI.ViewModels
{
    public class VitalSignPlotViewModel : ViewModelBase
    {
        private readonly DateTimeAxis xAxis;
        private readonly Dictionary<VitalSignType, Axis> vitalSignTypeAxes;
        private DateTime selectedTime;
        private readonly SelectedTime globalSelectedTime;
        private OxyPlot.Annotations.Annotation timeAnnotation;
        private Range<DateTime> loadedTimeRange;

        public VitalSignPlotViewModel(SelectedTime selectedTime)
        {
            globalSelectedTime = selectedTime;
            xAxis = new DateTimeAxis();
            vitalSignTypeAxes = CreateVitalSignTypeAxes();

            PlotModel = new PlotModel
            {
                Title = "Vital signs",
                Legends =
                {
                    new Legend
                    {
                        LegendPlacement = LegendPlacement.Inside,
                        LegendItemAlignment = HorizontalAlignment.Right,
                        LegendBackground = OxyColor.FromRgb(255, 255, 255),
                        LegendBorder = OxyColor.FromRgb(60, 60, 60),
                        LegendBorderThickness = 2
                    }
                },
                IsLegendVisible = true,
                Padding = new OxyThickness(30, 0, 10, 0)
            };
            PlotModel.Axes.Add(xAxis);
        }

        public PlotModel PlotModel { get; }

        public DateTime SelectedTime
        {
            get { return selectedTime; }
            private set
            {
                selectedTime = value;
                globalSelectedTime.Time = selectedTime;
                OnPropertyChanged();
            }
        }

        public void PlotData(
            IReadOnlyDictionary<SensorVitalSignType, TimeSeries<short>> vitalSignData,
            Range<DateTime> focusedTimeRange)
        {
            loadedTimeRange = focusedTimeRange;

            PlotModel.Series.Clear();
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(xAxis);
            PlotModel.Annotations.Clear();
            timeAnnotation = null;

            foreach (var sensorVitalSignType in vitalSignData.Keys)
            {
                var vitalSignType = sensorVitalSignType.VitalSignType;
                var timeSeries = vitalSignData[sensorVitalSignType];
                var yAxis = vitalSignTypeAxes[vitalSignType];
                if(!PlotModel.Axes.Contains(yAxis))
                    PlotModel.Axes.Add(yAxis);
                var series = new LineSeries
                {
                    YAxisKey = yAxis.Key,
                    Title = $"{sensorVitalSignType.VitalSignType} ({sensorVitalSignType.SensorType})",
                    LineLegendPosition = LineLegendPosition.End
                };
                series.Points.AddRange(ConvertTimeSeriesToPointList(timeSeries));
                series.MouseDown += Series_MouseDown;
                PlotModel.Series.Add(series);
            }
            xAxis.Zoom(DateTimeAxis.ToDouble(focusedTimeRange.From), DateTimeAxis.ToDouble(focusedTimeRange.To));
            PlotModel.InvalidatePlot(true);
        }

        private void Series_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left)
                return;
            if(e.ModifierKeys != OxyModifierKeys.Shift)
                return;
            if (!(sender is LineSeries))
                throw new Exception($"Event handler for series was hooked up to {sender.GetType()}");
            var plotPoint = ((LineSeries)sender).InverseTransform(e.Position);
            SelectedTime = DateTimeAxis.ToDateTime(plotPoint.X);
            if(timeAnnotation != null)
                PlotModel.Annotations.Remove(timeAnnotation);
            timeAnnotation = new LineAnnotation
            {
                Text = $"{SelectedTime:HH:mm:ss}",
                FontSize = 14,
                X = plotPoint.X,
                Type = LineAnnotationType.Vertical,
                Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                StrokeThickness = 3,
            };
            PlotModel.Annotations.Add(timeAnnotation);
            PlotModel.InvalidatePlot(false);
        }

        private IEnumerable<DataPoint> ConvertTimeSeriesToPointList(TimeSeries<short> timeSeries)
        {
            return timeSeries.Select(timePoint => DateTimeAxis.CreateDataPoint(timePoint.Time, timePoint.Value));
        }

        private Dictionary<VitalSignType, Axis> CreateVitalSignTypeAxes()
        {
            var bloodPressureAxis = CreateYAxis("NiBP", "mmHg", 75, 0, 200);
            return new Dictionary<VitalSignType, Axis>
            {
                { VitalSignType.HeartRate, CreateYAxis("HR","1/min", 0, 0, 200) },
                { VitalSignType.SpO2, CreateYAxis("SpO2", "%", 25, 70, 110) },
                { VitalSignType.RespirationRate, CreateYAxis("RR", "1/min", 50, 0, 50) },
                { VitalSignType.SystolicBloodPressure, bloodPressureAxis },
                { VitalSignType.DiastolicBloodPressure, bloodPressureAxis },
                { VitalSignType.MeanArterialPressure, bloodPressureAxis }
            };
        }

        private Axis CreateYAxis(string description, 
            string unitString, 
            double axisDistance, 
            double min = double.NaN, 
            double max = double.NaN)
        {
            return new LinearAxis
            {
                Title = $"{description} ({unitString})",
                Key = description,
                Position = AxisPosition.Left,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                AxisDistance = axisDistance,
                Minimum = min,
                Maximum = max
            };
        }

        public void SetAnnotations(IEnumerable<Annotation> annotations)
        {
            PlotModel.Annotations.Clear();
            if(timeAnnotation != null)
                PlotModel.Annotations.Add(timeAnnotation);
            if (loadedTimeRange != null)
                annotations = annotations.Where(annotation => loadedTimeRange.Contains(annotation.Timestamp));
            annotations.ForEach(annotation => PlotModel.Annotations.Add(new LineAnnotation
            {
                FontSize = 10,
                X = DateTimeAxis.ToDouble(annotation.Timestamp),
                Text = annotation.Title,
                ToolTip = annotation.Note,
                Type = LineAnnotationType.Vertical,
                LineStyle = LineStyle.Solid,
                Color = OxyColor.FromRgb(Colors.Purple.R, Colors.Purple.G, Colors.Purple.B),
                StrokeThickness = 4
            }));
            PlotModel.InvalidatePlot(false);
        }
    }
}