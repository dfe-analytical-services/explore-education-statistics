using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Institution : IObservationalUnit
    {
        [JsonProperty(PropertyName = "institution_id")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "institution_name")]
        public string Name { get; set; }

        private Institution()
        {
        }

        public Institution(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Institution Empty()
        {
            return new Institution(null, null);
        }

        protected bool Equals(Institution other)
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