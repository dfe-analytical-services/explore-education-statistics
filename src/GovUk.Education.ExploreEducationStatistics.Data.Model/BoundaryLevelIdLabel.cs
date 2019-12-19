namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class BoundaryLevelIdLabel
    {
        public BoundaryLevelIdLabel(long id, string label)
        {
            Id = id;
            Label = label;
        }

        public BoundaryLevelIdLabel()
        {
        }

        public long Id { get; set; }
        public string Label { get; set; }
    }
}