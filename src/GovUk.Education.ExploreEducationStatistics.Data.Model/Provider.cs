using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider: IObservationalUnit
    {

        [JsonProperty(PropertyName = "provider_urn")]
        public string Code { get; set; }

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
            Code = urn;
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
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Provider) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}