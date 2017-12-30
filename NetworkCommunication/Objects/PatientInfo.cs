namespace NetworkCommunication.Objects
{
    public class PatientInfo
    {
        public PatientInfo(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string LastName { get; }
        public string FirstName { get; }
    }
}