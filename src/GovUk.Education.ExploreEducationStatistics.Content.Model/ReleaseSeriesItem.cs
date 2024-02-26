using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseSeriesItem
{
    public Guid ReleaseId { get; set; }

    public bool IsLegacy { get; set; }

    public bool IsDraft { get; set; }

    public bool IsAmendment { get; set; }

    public int Order { get; set; }

    //public Guid LegacyReleaseId { get; set; } // @MarkFix
    //public string LegacyLinkDescription { get; set; }
    //public string LegacyLinkUrl { get; set; }
}
