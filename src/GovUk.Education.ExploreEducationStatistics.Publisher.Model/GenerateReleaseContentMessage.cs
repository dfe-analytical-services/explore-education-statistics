using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class GenerateReleaseContentMessage
    {
        public Guid? ReleaseId { get; set; }
        public Guid? ReleaseInfoId { get; set; }
    }
}