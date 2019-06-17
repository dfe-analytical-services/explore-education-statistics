using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /**
     * Regional School Commissioner Region
     */
    public class RscRegion : IObservationalUnit
    {
        [JsonIgnore] public string Code { get; set; }

        [JsonProperty(PropertyName = "rsc_region_lead_name")]
        public string Name => Code;

        public RscRegion(string code)
        {
            Code = code;
        }

        private RscRegion()
        {
        }

        public static RscRegion Empty()
        {
            return new RscRegion(null);
        }

        protected bool Equals(RscRegion other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RscRegion) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}