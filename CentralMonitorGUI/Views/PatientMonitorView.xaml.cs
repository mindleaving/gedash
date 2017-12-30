using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for MonitorListItemView.xaml
    /// </summary>
    public partial class PatientMonitorView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(PatientMonitorViewModel), typeof(PatientMonitorView), new PropertyMetadata(default(PatientMonitorViewModel)));

        public PatientMonitorView()
        {
            InitializeComponent();
        }

        public PatientMonitorViewModel ViewModel
        {
            get { return (PatientMonitorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
