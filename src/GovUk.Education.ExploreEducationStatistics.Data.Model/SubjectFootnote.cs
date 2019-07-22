namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class SubjectFootnote
    {
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public Footnote Footnote { get; set; }
        public long FootnoteId { get; set; }
    }
}