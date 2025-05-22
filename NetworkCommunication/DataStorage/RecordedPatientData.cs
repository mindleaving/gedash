using Commons.Physics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class RecordedPatientData
    {
        public RecordedPatientData(
            PatientInfo patientInfo, 
            IReadOnlyDictionary<SensorVitalSignType, TimeSeries<double>> vitalParameters, 
            IReadOnlyDictionary<SensorType, TimeSeries<short>> waveforms)
        {
            PatientInfo = patientInfo;
            VitalParameters = vitalParameters;
            Waveforms = waveforms;
        }

        public PatientInfo PatientInfo { get; }
        public IReadOnlyDictionary<SensorVitalSignType, TimeSeries<double>> VitalParameters { get; }
        public IReadOnlyDictionary<SensorType, TimeSeries<short>> Waveforms { get; }
    }
}