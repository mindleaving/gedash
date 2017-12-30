using System;
using System.Linq;
using System.Timers;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class VitalSignViewModel : ViewModelBase
    {
        public PatientMonitor Monitor { get; }
        private readonly Timer updateTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds) { AutoReset = true };

        public VitalSignViewModel(PatientMonitor monitor)
        {
            Monitor = monitor;
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Start();
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var heartRateValue = Monitor.VitalSignValues.FirstOrDefault(
                x => x.VitalSignType == VitalSignType.HeartRate);
            HeartRate = heartRateValue?.Value.ToString() ?? "X";

            var respirationRateValue = Monitor.VitalSignValues.FirstOrDefault(
                x => x.VitalSignType == VitalSignType.RespirationRate);
            RespirationRate = respirationRateValue?.Value.ToString() ?? "X";

            var spO2Value = Monitor.VitalSignValues.FirstOrDefault(
                x => x.VitalSignType == VitalSignType.SpO2);
            SpO2 = spO2Value?.Value.ToString() ?? "X";
        }

        private string heartRate = "X";
        private string respirationRate = "X";
        private string spO2 = "X";

        public string HeartRate
        {
            get { return heartRate; }
            set
            {
                heartRate = value; 
                OnPropertyChanged();
            }
        }

        public string RespirationRate
        {
            get { return respirationRate; }
            set
            {
                respirationRate = value; 
                OnPropertyChanged();
            }
        }

        public string SpO2
        {
            get { return spO2; }
            set
            {
                spO2 = value; 
                OnPropertyChanged();
            }
        }
    }
}