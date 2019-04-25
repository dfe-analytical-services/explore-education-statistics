using System;
using System.Collections.Generic;
using System.Text;

namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    public class FilesProcessorNotification
    {
        public string sourceFile { get; set; }
        public string fileName { get; set; }
        public Guid releaseId { get; set; }

    }
}
