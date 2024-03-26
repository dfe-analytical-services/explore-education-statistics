using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public class NotifyChangeMessage
{
    public bool Immediate { get; set; }
    public Guid ReleaseVersionId { get; set; }
    public Guid ReleaseStatusId { get; set; }

    public NotifyChangeMessage(bool immediate, Guid releaseVersionId, Guid releaseStatusId)
    {
        Immediate = immediate;
        ReleaseVersionId = releaseVersionId;
        ReleaseStatusId = releaseStatusId;
    }

    public override string ToString()
    {
        return $"{nameof(Immediate)}: {Immediate}, " +
               $"{nameof(ReleaseVersionId)}: {ReleaseVersionId}, " +
               $"{nameof(ReleaseStatusId)}: {ReleaseStatusId}";
    }
}
