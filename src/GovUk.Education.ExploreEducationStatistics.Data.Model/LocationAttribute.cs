#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public abstract class LocationAttribute : ILocationAttribute
    {
        public string? Code { get; }
        public string? Name { get; }

        protected LocationAttribute(string? code, string? name)
        {
            Code = code;
            Name = name;
        }

        protected bool Equals(LocationAttribute other)
        {
            return Code == other.Code && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LocationAttribute) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Name);
        }
    }
}
