using System.Collections.Generic;
using System.Windows.Input;
using CentralMonitorGUI.Views;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class PatientDatabaseViewModel : ViewModelBase
    {
        private readonly DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory;

        public PatientDatabaseViewModel(
            FileManager fileManager, 
            DataExplorerWindowViewModelFactory dataExplorerWindowViewModelFactory)
        {
            this.dataExplorerWindowViewModelFactory = dataExplorerWindowViewModelFactory;
            Patients = fileManager.GetAllPatients();
            OpenSelectedPatientCommand = new RelayCommand(OpenSelectedPatient);
        }

        public IList<PatientInfo> Patients { get; }

        private PatientInfo selectedPatient;
        public PatientInfo SelectedPatient
        {
            get => selectedPatient;
            set
            {
                selectedPatient = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSelectedPatientCommand { get; }

        private void OpenSelectedPatient()
        {
            if(SelectedPatient == null)
                return;
            var dataExplorerViewModel = dataExplorerWindowViewModelFactory.Create(SelectedPatient);
            var dialog = new DataExplorerWindow { ViewModel = dataExplorerViewModel };
            dialog.ShowDialog();
        }
    }
}
