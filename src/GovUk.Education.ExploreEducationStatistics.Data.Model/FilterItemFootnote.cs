namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class FilterItemFootnote
{
    public FilterItem FilterItem { get; set; }
    public Guid FilterItemId { get; set; }
    public Guid FilterGroupId { get; set; }
    public Guid FilterId { get; set; }
    public Guid SubjectId { get; set; }
    public Footnote Footnote { get; set; }
    public Guid FootnoteId { get; set; }
}
