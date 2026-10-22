namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class FilterFootnote
{
    public Guid FilterId { get; set; }
    public Guid SubjectId { get; set; }
    public Footnote Footnote { get; set; }
    public Guid FootnoteId { get; set; }
}
