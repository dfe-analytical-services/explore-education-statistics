using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class ImportMessagePayload
    {
        public Guid ReleaseId { get; set; }
        public string DataFileName { get; set; }
        public string MetaFileName { get; set; }
    }
}