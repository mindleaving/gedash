using System.Windows;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(default(MainViewModel)));

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
