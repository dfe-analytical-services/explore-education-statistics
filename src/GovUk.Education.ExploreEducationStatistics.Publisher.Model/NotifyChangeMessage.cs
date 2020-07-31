using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class NotifyChangeMessage
    {
        public bool Immediate { get; set; }
        public Guid ReleaseId { get; set; }

        public NotifyChangeMessage(bool immediate, Guid releaseId)
        {
            Immediate = immediate;
            ReleaseId = releaseId;
        }

        public override string ToString()
        {
            return $"{nameof(Immediate)}: {Immediate}, {nameof(ReleaseId)}: {ReleaseId}";
        }
    }
}