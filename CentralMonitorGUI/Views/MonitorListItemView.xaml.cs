using System.Windows;
using System.Windows.Controls;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for MonitorListItemView.xaml
    /// </summary>
    public partial class MonitorListItemView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MonitorListItemViewModel), typeof(MonitorListItemView), new PropertyMetadata(default(MonitorListItemViewModel)));

        public MonitorListItemView()
        {
            InitializeComponent();
        }

        public MonitorListItemViewModel ViewModel
        {
            get { return (MonitorListItemViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
