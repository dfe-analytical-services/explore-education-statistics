using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class CancelImportMessage
    {
        public Guid ReleaseId { get; set; }
        
        public string DataFileName { get; set; }
    }
}