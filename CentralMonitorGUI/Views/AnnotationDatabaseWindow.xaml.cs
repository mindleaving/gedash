using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using CentralMonitorGUI.ViewModels;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for AnnotationDatabaseWindow.xaml
    /// </summary>
    public partial class AnnotationDatabaseWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(AnnotationDatabaseViewModel), typeof(AnnotationDatabaseWindow), new PropertyMetadata(default(AnnotationDatabaseViewModel)));

        public AnnotationDatabaseWindow()
        {
            InitializeComponent();
        }

        public AnnotationDatabaseViewModel ViewModel
        {
            get { return (AnnotationDatabaseViewModel) GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                SetListViewSortDescription();
            }
        }

        private void SetListViewSortDescription()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(annotationListView.ItemsSource);
            if(view == null)
                return;
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(Annotation.Timestamp), ListSortDirection.Ascending));
        }
    }
}
