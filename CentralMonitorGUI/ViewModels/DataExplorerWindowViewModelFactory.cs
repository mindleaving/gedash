using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class DataExplorerWindowViewModelFactory
    {
        private readonly HistoryLoader historyLoader;

        public DataExplorerWindowViewModelFactory(HistoryLoader historyLoader)
        {
            this.historyLoader = historyLoader;
        }

        public DataExplorerWindowViewModel Create(PatientInfo patientInfo)
        {
            return new DataExplorerWindowViewModel(patientInfo, historyLoader);
        }
    }
}