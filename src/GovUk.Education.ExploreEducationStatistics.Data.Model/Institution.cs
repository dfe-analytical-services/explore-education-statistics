using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Institution
    {
        [JsonProperty(PropertyName = "institution_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "institution_name")]
        public string Name { get; set; }

        private Institution()
        {
        }

        public Institution(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public static Institution Empty()
        {
            return new Institution(null, null);
        }

        protected bool Equals(Institution other)
        {
            return string.Equals(Id, other.Id);
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
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}