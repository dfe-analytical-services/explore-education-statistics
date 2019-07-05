using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthorityDistrict: IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private LocalAuthorityDistrict()
        {
        }

        public LocalAuthorityDistrict(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static LocalAuthorityDistrict Empty()
        {
            return new LocalAuthorityDistrict(null, null);
        }

        protected bool Equals(LocalAuthorityDistrict other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LocalAuthorityDistrict) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}