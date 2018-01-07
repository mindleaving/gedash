using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for VitalSignPlotView.xaml
    /// </summary>
    public partial class VitalSignPlotView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(VitalSignPlotViewModel), typeof(VitalSignPlotView), new PropertyMetadata(default(VitalSignPlotViewModel)));

        public VitalSignPlotView()
        {
            InitializeComponent();
        }

        public VitalSignPlotViewModel ViewModel
        {
            get { return (VitalSignPlotViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
