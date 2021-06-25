using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class NotifyChangeMessage
    {
        public bool Immediate { get; set; }
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }

        public NotifyChangeMessage(bool immediate, Guid releaseId, Guid releaseStatusId)
        {
            Immediate = immediate;
            ReleaseId = releaseId;
            ReleaseStatusId = releaseStatusId;
        }

        public override string ToString()
        {
            return $"{nameof(Immediate)}: {Immediate}, " +
            $"{nameof(ReleaseId)}: {ReleaseId}, " +
            $"{nameof(ReleaseStatusId)}: {ReleaseStatusId}";
        }
    }
}
