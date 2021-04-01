namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class EnglishDevolvedArea : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public EnglishDevolvedArea(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static EnglishDevolvedArea Empty()
        {
            return new EnglishDevolvedArea(null, null);
        }

        protected bool Equals(EnglishDevolvedArea other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EnglishDevolvedArea) obj);
        }

        public override int GetHashCode()
        {
            return Code != null ? Code.GetHashCode() : 0;
        }
    }
}
