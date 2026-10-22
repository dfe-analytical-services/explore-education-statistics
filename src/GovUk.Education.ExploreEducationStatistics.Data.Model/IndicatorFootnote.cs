namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class IndicatorFootnote
{
    public Guid IndicatorId { get; set; }
    public Guid IndicatorGroupId { get; set; }
    public Guid SubjectId { get; set; }
    public Footnote Footnote { get; set; }
    public Guid FootnoteId { get; set; }
}
