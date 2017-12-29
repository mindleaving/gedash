﻿using System;
using System.Timers;

namespace CentralMonitorGUI.ViewModels
{
    public class UpdateTrigger
    {
        private readonly TimeSpan trigPeriod;
        private readonly Timer timer;

        public UpdateTrigger(TimeSpan trigPeriod)
        {
            this.trigPeriod = trigPeriod;
            timer = new Timer(trigPeriod.TotalMilliseconds) { AutoReset = true };
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trig?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Trig;
    }
}