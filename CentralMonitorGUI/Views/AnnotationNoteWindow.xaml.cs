using System.Windows;
using CentralMonitorGUI.ViewModels;

namespace CentralMonitorGUI.Views
{
    /// <summary>
    /// Interaction logic for AnnotationNoteWindow.xaml
    /// </summary>
    public partial class AnnotationNoteWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(AnnotationNoteViewModel), typeof(AnnotationNoteWindow), new PropertyMetadata(default(AnnotationNoteViewModel)));

        public AnnotationNoteWindow()
        {
            InitializeComponent();
        }

        public AnnotationNoteViewModel ViewModel
        {
            get { return (AnnotationNoteViewModel) GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                if(value == null) return;
                value.CloseRequested += Value_CloseRequested;
            }
        }

        private void Value_CloseRequested(object sender, bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
