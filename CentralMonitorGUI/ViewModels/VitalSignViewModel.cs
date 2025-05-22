using System.Timers;
using NetworkCommunication.Objects;
using Timer = System.Timers.Timer;

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

            var systolicBloodPressure = Monitor.VitalSignValues.FirstOrDefault(
                x => x.VitalSignType == VitalSignType.SystolicBloodPressure);
            var diastolicBloodPressure = Monitor.VitalSignValues.FirstOrDefault(
                x => x.VitalSignType == VitalSignType.DiastolicBloodPressure);
            NiBP = $"{systolicBloodPressure?.Value.ToString() ?? "X"} / {diastolicBloodPressure?.Value.ToString() ?? "X"}";
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
        private string niBp = "X / X";
        public string NiBP
        {
            get => niBp;
            set
            {
                niBp = value;
                OnPropertyChanged();
            }
        }
    }
}