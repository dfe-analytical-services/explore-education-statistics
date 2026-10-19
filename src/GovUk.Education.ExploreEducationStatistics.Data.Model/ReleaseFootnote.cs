#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class ReleaseFootnote
{
    public Footnote Footnote { get; set; } = null!;

    public Guid FootnoteId { get; set; }

    public Guid ReleaseVersionId { get; set; }
}
