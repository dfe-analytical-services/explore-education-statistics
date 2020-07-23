using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class RetryStageMessage
    {
        public Guid ReleaseId { get; set; }
        public RetryStage Stage { get; set; }

        public RetryStageMessage(Guid releaseId, RetryStage stage)
        {
            ReleaseId = releaseId;
            Stage = stage;
        }

        public override string ToString()
        {
            return $"{nameof(ReleaseId)}: {ReleaseId}";
        }
    }
}