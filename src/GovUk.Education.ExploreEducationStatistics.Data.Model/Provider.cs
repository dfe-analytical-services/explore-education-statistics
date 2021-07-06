namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private Provider()
        {
        }

        public Provider(string ukprn, string name)
        {
            Code = ukprn;
            Name = name;
        }

        public static Provider Empty()
        {
            return new Provider(null, null);
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
