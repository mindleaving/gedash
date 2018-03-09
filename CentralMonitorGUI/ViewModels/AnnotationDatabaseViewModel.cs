using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Commons.Wpf;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.ViewModels
{
    public class AnnotationDatabaseViewModel : ViewModelBase
    {
        private readonly AnnotationDatabase annotationDatabase;
        private readonly Action<Annotation> showAnnotation;
        private readonly Action updateAnnotations;

        public AnnotationDatabaseViewModel(
            AnnotationDatabase annotationDatabase,
            Action<Annotation> showAnnotation,
            Action updateAnnotations)
        {
            this.annotationDatabase = annotationDatabase;
            this.showAnnotation = showAnnotation;
            this.updateAnnotations = updateAnnotations;

            ShowAnnotationCommand = new RelayCommand(ShowAnnotation);
            DeleteAnnotationCommand = new RelayCommand(DeleteAnnotation);

            Annotations = new ObservableCollection<Annotation>(annotationDatabase.Annotations);
        }

        public ObservableCollection<Annotation> Annotations { get; }
        private Annotation selectedAnnotation;
        public Annotation SelectedAnnotation
        {
            get => selectedAnnotation;
            set
            {
                selectedAnnotation = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowAnnotationCommand { get; }
        public ICommand DeleteAnnotationCommand { get; }


        private void ShowAnnotation()
        {
            if(SelectedAnnotation == null)
                return;
            showAnnotation(SelectedAnnotation);
        }

        private void DeleteAnnotation()
        {
            if (SelectedAnnotation == null)
                return;
            var confirmationDialog = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedAnnotation.Title}'?",
                "Confirm remove annotation",
                MessageBoxButton.YesNo);
            if(confirmationDialog != MessageBoxResult.Yes)
                return;
            annotationDatabase.Remove(SelectedAnnotation);
            Annotations.Remove(SelectedAnnotation);
            updateAnnotations();
        }
    }
}