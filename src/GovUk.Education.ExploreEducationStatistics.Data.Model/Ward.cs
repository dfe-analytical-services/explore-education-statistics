namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Ward : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private Ward()
        {
        }

        public Ward(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Ward Empty()
        {
            return new Ward(null, null);
        }

        protected bool Equals(Ward other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Ward) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}