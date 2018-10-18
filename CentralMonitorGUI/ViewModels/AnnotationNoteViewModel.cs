using System;
using System.Windows.Input;

namespace CentralMonitorGUI.ViewModels
{
    public class AnnotationNoteViewModel : ViewModelBase
    {
        public AnnotationNoteViewModel(DateTime timestamp)
        {
            Timestamp = timestamp;
            CancelCommand = new RelayCommand(Cancel);
            SaveCommand = new RelayCommand(Save);
        }

        public event EventHandler<bool?> CloseRequested;

        public DateTime Timestamp { get; }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private string note;
        public string Note
        {
            get { return note; }
            set
            {
                note = value; 
                OnPropertyChanged();
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        private void Save()
        {
            CloseRequested?.Invoke(this, true);
        }
    }
}