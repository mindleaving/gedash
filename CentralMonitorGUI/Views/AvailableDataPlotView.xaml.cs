using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for AvailableDataPlotView.xaml
    /// </summary>
    public partial class AvailableDataPlotView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(AvailableDataPlotViewModel), typeof(AvailableDataPlotView), new PropertyMetadata(default(AvailableDataPlotViewModel)));

        public AvailableDataPlotView()
        {
            InitializeComponent();
        }

        public AvailableDataPlotViewModel ViewModel
        {
            get { return (AvailableDataPlotViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
