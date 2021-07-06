using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public School()
        {
        }

        public School(string urn, string name)
        {
            Code = urn;
            Name = name;
        }

        public static School Empty()
        {
            return new School(null, null);
        }

        protected bool Equals(School other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((School) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}
