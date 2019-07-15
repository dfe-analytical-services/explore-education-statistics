namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class OpportunityArea : IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private OpportunityArea()
        {
        }

        public OpportunityArea(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static OpportunityArea Empty()
        {
            return new OpportunityArea(null, null);
        }

        protected bool Equals(OpportunityArea other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OpportunityArea) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}