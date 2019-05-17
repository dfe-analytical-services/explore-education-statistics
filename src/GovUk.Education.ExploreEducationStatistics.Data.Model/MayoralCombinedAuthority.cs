using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class MayoralCombinedAuthority : IObservationalUnit
    {
        [JsonProperty(PropertyName = "mayoral_combined_authority_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "mayoral_combined_authority_name")]
        public string Name { get; set; }

        private MayoralCombinedAuthority()
        {
        }

        public MayoralCombinedAuthority(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static MayoralCombinedAuthority Empty()
        {
            return new MayoralCombinedAuthority(null, null);
        }

        protected bool Equals(MayoralCombinedAuthority other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MayoralCombinedAuthority) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}