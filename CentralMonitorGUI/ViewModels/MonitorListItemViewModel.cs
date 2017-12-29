using System.Collections.ObjectModel;

namespace CentralMonitorGUI.ViewModels
{
    public class MonitorListItemViewModel
    {
        public ObservableCollection<WaveformViewModel> EnabledWaveforms { get; } = new ObservableCollection<WaveformViewModel>();
    }
}