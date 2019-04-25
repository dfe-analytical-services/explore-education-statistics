using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthority
    {
        [JsonProperty(PropertyName = "new_la_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "old_la_code")]
        public string Old_Code { get; set; }

        [JsonProperty(PropertyName = "la_name")]
        public string Name { get; set; }

        public LocalAuthority()
        {
        }

        public LocalAuthority(string code, string oldCode, string name)
        {
            Code = code;
            Old_Code = oldCode;
            Name = name;
        }

        public static LocalAuthority Empty()
        {
            return new LocalAuthority(null, null, null);
        }

        protected bool Equals(LocalAuthority other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LocalAuthority) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}