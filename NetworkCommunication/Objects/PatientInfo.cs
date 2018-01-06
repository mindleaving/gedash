using System;

namespace NetworkCommunication.Objects
{
    public class PatientInfo : IEquatable<PatientInfo>
    {
        public PatientInfo(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string LastName { get; }
        public string FirstName { get; }

        public bool Equals(PatientInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(LastName, other.LastName) 
                   && string.Equals(FirstName, other.FirstName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PatientInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LastName != null ? LastName.GetHashCode() : 0) * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
            }
        }

        public static bool operator ==(PatientInfo left, PatientInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PatientInfo left, PatientInfo right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{LastName}, {FirstName}";
        }
    }
}