using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;
public class ReleaseOrder
{
    public Guid ReleaseId { get; set; }

    public bool IsLegacy { get; set; }

    public bool IsDraft { get; set; }

    public bool IsAmendment { get; set; }

    public int Order { get; set; }
}
