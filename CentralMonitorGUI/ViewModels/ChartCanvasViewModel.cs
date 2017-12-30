using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CentralMonitorGUI.Annotations;
using NetworkCommunication;

namespace CentralMonitorGUI.ViewModels
{
    public class ChartCanvasViewModel : ViewModelBase
    {
        private double canvasWidth = 300;

        public ChartCanvasViewModel(int valueCount)
        {
            if(valueCount < 2)
                throw new ArgumentException("Less than 2 values is not supported");
            var xStep = CanvasWidth / (valueCount - 1);
            X = Enumerable.Range(0, valueCount)
                .Select(idx => new ObservableValue(idx * xStep))
                .ToList();
            Values = Enumerable.Range(0, valueCount)
                .Select(idx => new ObservableValue(StaticRng.RNG.Next(0, 100)))
                .ToList();
            Lines = CreateLines();
        }

        private List<Line> CreateLines()
        {
            if(Values.Count < 2)
                return new List<Line>();
            var lines = new List<Line>();
            for (int valueIdx = 0; valueIdx < Values.Count-1; valueIdx++)
            {
                var firstX = X[valueIdx];
                var secondX = X[valueIdx + 1];
                var firstValue = Values[valueIdx];
                var secondValue = Values[valueIdx + 1];

                var from = new ObservablePoint(firstX, firstValue);
                var to = new ObservablePoint(secondX, secondValue);
                var line = new Line(from, to);
                lines.Add(line);
            }

            return lines;
        }

        private IReadOnlyList<ObservableValue> X { get; }
        public IReadOnlyList<ObservableValue> Values { get; }
        public IReadOnlyList<Line> Lines { get; }

        public double CanvasWidth
        {
            get { return canvasWidth; }
            set
            {
                canvasWidth = value;
                OnPropertyChanged();
                if (double.IsNaN(value))
                    return;
                var xStep = value / (Values.Count - 1);
                for (int xIdx = 0; xIdx < X.Count; xIdx++)
                {
                    X[xIdx].Value = xIdx * xStep;
                }
            }
        }
    }

    public class Line
    {
        public Line(ObservablePoint from, ObservablePoint to)
        {
            From = from;
            To = to;
        }

        public ObservablePoint From { get; }
        public ObservablePoint To { get; }
    }

    public class ObservableValue : INotifyPropertyChanged
    {
        private double value;

        public ObservableValue(double value)
        {
            Value = value;
        }

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value; 
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservablePoint
    {
        public ObservablePoint(ObservableValue x, ObservableValue y)
        {
            X = x;
            Y = y;
        }

        public ObservableValue X { get; }
        public ObservableValue Y { get; }
    }
}