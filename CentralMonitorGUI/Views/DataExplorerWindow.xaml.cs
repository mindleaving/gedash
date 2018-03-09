using System.Windows;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for DataExplorerWindow.xaml
    /// </summary>
    public partial class DataExplorerWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataExplorerWindowViewModel), typeof(DataExplorerWindow), new PropertyMetadata(default(DataExplorerWindowViewModel)));

        public DataExplorerWindow()
        {
            InitializeComponent();
        }

        public DataExplorerWindowViewModel ViewModel
        {
            get { return (DataExplorerWindowViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
