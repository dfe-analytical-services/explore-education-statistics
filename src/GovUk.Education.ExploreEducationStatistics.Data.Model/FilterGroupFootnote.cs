namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class FilterGroupFootnote
{
    public Guid FilterGroupId { get; set; }
    public Guid FilterId { get; set; }
    public Guid SubjectId { get; set; }
    public Footnote Footnote { get; set; }
    public Guid FootnoteId { get; set; }
}
