#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class ReleaseFootnote
{
    public Footnote Footnote { get; set; } = null!;

    public Guid FootnoteId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }
}
