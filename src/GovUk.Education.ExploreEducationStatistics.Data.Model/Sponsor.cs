namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Sponsor : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private Sponsor()
        {
        }

        public Sponsor(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Sponsor Empty()
        {
            return new Sponsor(null, null);
        }

        protected bool Equals(Sponsor other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Sponsor) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}