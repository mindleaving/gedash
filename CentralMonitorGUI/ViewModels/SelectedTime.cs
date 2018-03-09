using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CentralMonitorGUI.Annotations;

namespace CentralMonitorGUI.ViewModels
{
    public class SelectedTime : INotifyPropertyChanged
    {
        private DateTime time;
        public DateTime Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}