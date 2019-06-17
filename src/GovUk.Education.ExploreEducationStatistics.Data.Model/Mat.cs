using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Mat : IObservationalUnit
    {
        [JsonProperty(PropertyName = "mat_id")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "mat_name")]
        public string Name { get; set; }

        private Mat()
        {
        }

        public Mat(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Mat Empty()
        {
            return new Mat(null, null);
        }

        protected bool Equals(Mat other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Mat) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}