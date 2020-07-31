using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishReleaseContentMessage
    {
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }

        public PublishReleaseContentMessage(Guid releaseId, Guid releaseStatusId)
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