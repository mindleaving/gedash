using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class DataExplorerWindowViewModelFactory
    {
        private readonly HistoryLoader historyLoader;
        private readonly FileManager fileManager;

        public DataExplorerWindowViewModelFactory(
            HistoryLoader historyLoader, 
            FileManager fileManager)
        {
            this.historyLoader = historyLoader;
            this.fileManager = fileManager;
        }

        public DataExplorerWindowViewModel Create(PatientInfo patientInfo)
        {
            var annotationDatabase = new AnnotationDatabase(fileManager, patientInfo);
            return new DataExplorerWindowViewModel(patientInfo, historyLoader, annotationDatabase);
        }
    }
}