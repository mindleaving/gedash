using System.Windows;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for PatientDatabaseWindow.xaml
    /// </summary>
    public partial class PatientDatabaseWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(PatientDatabaseViewModel), typeof(PatientDatabaseWindow), new PropertyMetadata(default(PatientDatabaseViewModel)));

        public PatientDatabaseWindow()
        {
            InitializeComponent();
        }

        public PatientDatabaseViewModel ViewModel
        {
            get { return (PatientDatabaseViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
