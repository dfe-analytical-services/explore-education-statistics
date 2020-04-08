using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishReleaseContentImmediateMessage
    {
        public Guid ReleaseId { get; set; }
        public Guid? ReleaseStatusId { get; set; }

        public PublishReleaseContentImmediateMessage(Guid releaseId)
        {
            ReleaseId = releaseId;
        }

        public PublishReleaseContentImmediateMessage(Guid releaseId, Guid? releaseStatusId)
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