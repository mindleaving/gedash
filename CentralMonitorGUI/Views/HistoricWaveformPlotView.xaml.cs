using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for HistoricWaveformPlotView.xaml
    /// </summary>
    public partial class HistoricWaveformPlotView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(HistoricWaveformPlotViewModel), typeof(HistoricWaveformPlotView), new PropertyMetadata(default(HistoricWaveformPlotViewModel)));

        public HistoricWaveformPlotView()
        {
            InitializeComponent();
        }

        public HistoricWaveformPlotViewModel ViewModel
        {
            get { return (HistoricWaveformPlotViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
