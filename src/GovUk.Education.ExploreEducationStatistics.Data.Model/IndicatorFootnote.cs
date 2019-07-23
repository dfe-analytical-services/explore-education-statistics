namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class IndicatorFootnote
    {
        public Indicator Indicator { get; set; }
        public long IndicatorId { get; set; }
        public Footnote Footnote { get; set; }
        public long FootnoteId { get; set; }
    }
}