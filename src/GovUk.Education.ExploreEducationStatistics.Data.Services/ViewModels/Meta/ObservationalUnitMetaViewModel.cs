using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitMetaViewModel : LabelValue
    {
        public string Level { get; set; }

        protected bool Equals(ObservationalUnitMetaViewModel other)
        {
            return base.Equals(other) && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObservationalUnitMetaViewModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Level != null ? Level.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}