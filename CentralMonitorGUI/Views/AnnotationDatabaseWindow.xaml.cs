using System.Windows;
using CentralMonitorGUI.ViewModels;

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
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
