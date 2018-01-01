using System.Collections.Generic;
using System.Net;

namespace NetworkCommunication.Objects
{
    public interface IMonitorDatabase
    {
        IReadOnlyDictionary<IPAddress, PatientMonitor> Monitors { get; }
    }
}