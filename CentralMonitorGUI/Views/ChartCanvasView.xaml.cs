using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for ChartCanvas.xaml
    /// </summary>
    public partial class ChartCanvasView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(ChartCanvasViewModel), typeof(ChartCanvasView), new PropertyMetadata(default(ChartCanvasViewModel)));

        public ChartCanvasView()
        {
            InitializeComponent();
        }

        public ChartCanvasViewModel ViewModel
        {
            get { return (ChartCanvasViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
