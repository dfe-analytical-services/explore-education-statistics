using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public class PublishReleaseContentMessage
{
    public Guid ReleaseVersionId { get; set; }
    public Guid ReleaseStatusId { get; set; }

    public PublishReleaseContentMessage(Guid releaseVersionId, Guid releaseStatusId)
    {
        ReleaseVersionId = releaseVersionId;
        ReleaseStatusId = releaseStatusId;
    }

    public override string ToString()
    {
        return $"{nameof(ReleaseVersionId)}: {ReleaseVersionId}, {nameof(ReleaseStatusId)}: {ReleaseStatusId}";
    }
}
