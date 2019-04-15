using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Region
    {
        [JsonProperty(PropertyName = "region_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "region_name")]
        public string Name { get; set; }

        private Region()
        {
        }

        public Region(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Region Empty()
        {
            return new Region(null, null);
        }

        protected bool Equals(Region other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Region) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}