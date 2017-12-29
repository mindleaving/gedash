using NetworkCommunication.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CentralMonitorGUI.ViewModels
{
    public class PatientMonitor
    {
        public string WardName { get; set; }
        public string BedName { get; set; }
        public PatientInfo PatientInfo { get; set; }
        public Dictionary<SensorType, ObservableCollection<short>> SensorData { get; } = new Dictionary<SensorType, ObservableCollection<short>>();
    }
}