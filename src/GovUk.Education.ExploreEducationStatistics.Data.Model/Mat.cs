using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Mat
    {
        [JsonProperty(PropertyName = "mat_chain_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "mat_chain_name")]
        public string Name { get; set; }

        private Mat()
        {
        }

        public Mat(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public static Mat Empty()
        {
            return new Mat(null, null);
        }

        protected bool Equals(Mat other)
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