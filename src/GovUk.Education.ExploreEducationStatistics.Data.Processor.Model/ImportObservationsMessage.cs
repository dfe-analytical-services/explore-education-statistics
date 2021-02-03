using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class ImportObservationsMessage
    {
        public Guid Id;
        public string ObservationsFilePath { get; set; }
        public int BatchNo { get; set; }
    }
}