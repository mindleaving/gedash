using System;
using System.Linq;
using System.Windows.Media;
using Commons.Mathematics;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CentralMonitorGUI.ViewModels
{
    public class AvailableDataPlotViewModel
    {
        private readonly PatientInfo patientInfo;
        private readonly HistoryLoader historyLoader;
        private bool selectRangeStart = true;
        private DateTime rangeStart;
        private double y = 1.0;

        public AvailableDataPlotViewModel(PatientInfo patientInfo, HistoryLoader historyLoader)
        {
            this.patientInfo = patientInfo;
            this.historyLoader = historyLoader;
            PlotModel = new PlotModel { Title = "Available data" };
            PlotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Minimum = 0,
                Maximum = 2
            });
        }

        public PlotModel PlotModel { get; }
        public Range<DateTime>? SelectedTimeRange { get; private set; }

        public void UpdateDataRange()
        {
            var availableDataRanges = historyLoader.GetAvailableDataForPatient(patientInfo);
            PlotModel.Series.Clear();
            if (!availableDataRanges.Any())
            {
                PlotModel.InvalidatePlot(true);
                return;
            }
            var color = Colors.Blue;
            var series = new LineSeries
            {
                StrokeThickness = 10,
                Color = OxyColor.FromRgb(color.R, color.G, color.B)
            };
            for (int rangeIdx = 0; rangeIdx < availableDataRanges.Count; rangeIdx++)
            {
                var range = availableDataRanges[rangeIdx];
                series.Points.Add(DateTimeAxis.CreateDataPoint(range.From, y));
                series.Points.Add(DateTimeAxis.CreateDataPoint(range.To, y));
                if (rangeIdx + 1 < availableDataRanges.Count)
                {
                    var nextRange = availableDataRanges[rangeIdx + 1];
                    var rangeSeparation = nextRange.From - range.To;
                    var halfwayDateTime = range.To.AddHours(0.5 * rangeSeparation.TotalHours);
                    series.Points.Add(DateTimeAxis.CreateDataPoint(halfwayDateTime, double.NaN));
                }
            }
            series.MouseDown += Series_MouseDown;
            PlotModel.Series.Add(series);

            SelectedTimeRange = new Range<DateTime>(availableDataRanges.First().From, availableDataRanges.Last().To);
            var dataStart = DateTimeAxis.ToDouble(SelectedTimeRange.From);
            var dataEnd = DateTimeAxis.ToDouble(SelectedTimeRange.To);
            PlotModel.Axes.Single(axis => axis.IsHorizontal()).Zoom(dataStart, dataEnd);
            PlotModel.InvalidatePlot(true);
        }

        private void Series_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if(e.ChangedButton != OxyMouseButton.Left)
                return;
            if(!(sender is LineSeries))
                return;
            var clickedValue = ((LineSeries) sender).InverseTransform(e.Position);
            var clickedTime = DateTimeAxis.ToDateTime(clickedValue.X);
            if (selectRangeStart)
            {
                PlotModel.Annotations.Clear();
                PlotModel.Annotations.Add(new LineAnnotation
                {
                    Text = "Start",
                    Type = LineAnnotationType.Vertical,
                    X = clickedValue.X,
                    Selectable = false,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 4
                });
                PlotModel.InvalidatePlot(false);
                rangeStart = clickedTime;
            }
            else
            {
                PlotModel.Annotations.Add(new LineAnnotation
                {
                    Text = "End",
                    Type = LineAnnotationType.Vertical,
                    X = clickedValue.X,
                    Selectable = false,
                    Color = OxyColor.FromRgb(Colors.Red.R, Colors.Red.G, Colors.Red.B),
                    StrokeThickness = 4
                });
                PlotModel.InvalidatePlot(false);
                SelectedTimeRange = new Range<DateTime>(
                    clickedTime < rangeStart ? clickedTime : rangeStart,
                    clickedTime < rangeStart ? rangeStart : clickedTime);
            }
            selectRangeStart = !selectRangeStart;
        }
    }
}