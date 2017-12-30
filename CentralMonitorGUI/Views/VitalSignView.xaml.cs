using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for VitalSignView.xaml
    /// </summary>
    public partial class VitalSignView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(VitalSignViewModel), typeof(VitalSignView), new PropertyMetadata(default(VitalSignViewModel)));

        public VitalSignView()
        {
            InitializeComponent();
        }

        public VitalSignViewModel ViewModel
        {
            get { return (VitalSignViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
