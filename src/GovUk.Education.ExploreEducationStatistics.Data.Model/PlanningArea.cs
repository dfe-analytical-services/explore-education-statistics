namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class PlanningArea: IObservationalUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private PlanningArea()
        {
        }

        public PlanningArea(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static PlanningArea Empty()
        {
            return new PlanningArea(null, null);
        }

        protected bool Equals(PlanningArea other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlanningArea) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}