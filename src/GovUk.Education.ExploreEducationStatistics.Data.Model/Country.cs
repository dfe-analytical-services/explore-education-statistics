namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Country : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private Country()
        {
        }

        public Country(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Country Empty()
        {
            return new Country(null, null);
        }

        protected bool Equals(Country other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Country) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}