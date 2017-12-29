using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for WaveformView.xaml
    /// </summary>
    public partial class WaveformView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(WaveformViewModel), typeof(WaveformView), new PropertyMetadata(default(WaveformViewModel)));

        public WaveformView()
        {
            InitializeComponent();
        }

        public WaveformViewModel ViewModel
        {
            get { return (WaveformViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
