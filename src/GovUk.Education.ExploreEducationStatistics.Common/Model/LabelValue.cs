namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class LabelValue
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public LabelValue()
        {
        }

        public LabelValue(string label, string value)
        {
            Label = label;
            Value = value;
        }

        protected bool Equals(LabelValue other)
        {
            return Label == other.Label && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LabelValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Label != null ? Label.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}