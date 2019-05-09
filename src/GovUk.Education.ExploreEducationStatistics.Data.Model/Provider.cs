using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider
    {

        [JsonProperty(PropertyName = "provider_urn")]
        public string Urn { get; set; }

        [JsonProperty(PropertyName = "provider_ukprn")]
        public string Ukprn { get; set; }

        [JsonProperty(PropertyName = "provider_upin")]
        public string Upin { get; set; }

        [JsonProperty(PropertyName = "provider_name")]
        public string Name { get; set; }
        
        private Provider()
        {
        }

        public Provider(string urn, string ukprn, string upin, string name)
        {
            Urn = urn;
            Ukprn = ukprn;
            Upin = upin;
            Name = name;
        }

        public static Provider Empty()
        {
            return new Provider(null, null, null, null);
        }

        protected bool Equals(Provider other)
        {
            return string.Equals(Urn, other.Urn);
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
            return (Urn != null ? Urn.GetHashCode() : 0);
        }
    }
}