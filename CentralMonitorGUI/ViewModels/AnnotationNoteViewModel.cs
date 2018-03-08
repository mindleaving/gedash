using System;
using System.Windows.Input;
using Commons.Wpf;

namespace CentralMonitorGUI.ViewModels
{
    public enum AnnotationType
    {
        VitalSigns,
        Waveforms
    }
    public class AnnotationNoteViewModel : ViewModelBase
    {
        public AnnotationNoteViewModel(DateTime timestamp, AnnotationType annotationType)
        {
            Timestamp = timestamp.ToString("yyyy-MM-dd") + " - ";
            AnnotationType = annotationType;
            CancelCommand = new RelayCommand(Cancel);
            SaveCommand = new RelayCommand(Save);
        }

        public event EventHandler<bool?> CloseRequested;

        public string Timestamp { get; }
        public AnnotationType AnnotationType { get; }

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