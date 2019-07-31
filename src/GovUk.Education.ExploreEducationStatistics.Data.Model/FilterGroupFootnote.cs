namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroupFootnote
    {
        public FilterGroup FilterGroup { get; set; }
        public long FilterGroupId { get; set; }
        public Footnote Footnote { get; set; }
        public long FootnoteId { get; set; }
    }
}