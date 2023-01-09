using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishReleaseDataMessage
    {
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }

        public PublishReleaseDataMessage(Guid releaseId, Guid releaseStatusId)
        {
            ReleaseId = releaseId;
            ReleaseStatusId = releaseStatusId;
        }

        public override string ToString()
        {
            return $"{nameof(ReleaseId)}: {ReleaseId}, {nameof(ReleaseStatusId)}: {ReleaseStatusId}";
        }
    }
}