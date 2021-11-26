#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public record LabelValue
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        protected LabelValue()
        {
        }

        public LabelValue(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}