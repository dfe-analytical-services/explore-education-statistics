namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Multi academy trust
    /// </summary>
    public class Mat : IObservationalUnit
    {
        public string Code { get; set; }
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