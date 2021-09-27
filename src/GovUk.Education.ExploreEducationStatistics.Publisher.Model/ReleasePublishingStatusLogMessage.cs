using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleasePublishingStatusLogMessage
    {
        public string Message { get; set; }
        public DateTime On { get; set; }

        public ReleasePublishingStatusLogMessage(string message)
        {
            Message = message;
            On = DateTime.UtcNow;
        }
    }
}