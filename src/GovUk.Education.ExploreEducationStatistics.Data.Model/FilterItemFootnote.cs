namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class FilterItemFootnote
{
    public Guid FilterItemId { get; set; }
    public Guid SubjectId { get; set; }
    public Footnote Footnote { get; set; }
    public Guid FootnoteId { get; set; }
}
