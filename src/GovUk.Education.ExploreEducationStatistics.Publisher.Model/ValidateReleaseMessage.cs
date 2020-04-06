using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ValidateReleaseMessage
    {
        public bool Immediate { get; set; }
        public Guid ReleaseId { get; set; }
    }
}